// Copyright 2015 Andreas Jakl, Tieto Corporation. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// Based on the Google Eddystone specification, 
// available under Apache License, Version 2.0 from
// https://github.com/google/eddystone
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
//    http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;

namespace UniversalBeaconLibrary.Beacon
{
    public class Beacon
    {
        private readonly Guid _eddystoneGuid = new Guid("0000FEAA-0000-1000-8000-00805F9B34FB");

        public enum BeaconTypeEnum
        {
            /// <summary>
            /// Bluetooth LE advertisment that is not recognized as one of the beacon formats
            /// supported by this library.
            /// </summary>
            Unknown,
            /// <summary>
            /// Beacon conforming to the Google Eddystone specification.
            /// </summary>
            Eddystone,
            /// <summary>
            /// Beacon conforming to the Apple iBeacon specification.
            /// iBeacon is a Trademark of Apple Inc.
            /// Note: the beacon broadcast payload is not parsed by this library.
            /// </summary>
            iBeacon 
        }

        public BeaconTypeEnum BeaconType { get; set; } = BeaconTypeEnum.Unknown;

        public List<BeaconFrameBase> BeaconFrames { get; set; } = new List<BeaconFrameBase>();

        public short Rssi { get; set; }
        public ulong BluetoothAddress { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public Beacon()
        {
            
        }

        public Beacon(BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            BluetoothAddress = btAdv.BluetoothAddress;
            UpdateBeacon(btAdv);
        }

        public void UpdateBeacon(BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            if (btAdv.BluetoothAddress != BluetoothAddress)
            {
                throw new BeaconException("Bluetooth address of beacon does not match - not updating beacon information");
            }

            Rssi = btAdv.RawSignalStrengthInDBm;
            Timestamp = btAdv.Timestamp;

            //Debug.WriteLine($"Beacon detected (Strength: {Rssi}): Address: {BluetoothAddress}");

            // Check if beacon advertisement contains any actual usable data
            if (btAdv.Advertisement == null) return;

            if (btAdv.Advertisement.ServiceUuids.Any())
            {
                foreach (var serviceUuid in btAdv.Advertisement.ServiceUuids)
                {
                    // If we have multiple service UUIDs and already recognized a beacon type, 
                    // don't overwrite it with another service Uuid.
                    if (BeaconType == BeaconTypeEnum.Unknown)
                    {
                        BeaconType = serviceUuid.Equals(_eddystoneGuid)
                            ? BeaconTypeEnum.Eddystone
                            : BeaconTypeEnum.Unknown;
                    }
                }
            }
            else
            {
                //Debug.WriteLine("Bluetooth LE device does not send Service UUIDs");
            }

            // Data sections
            if (btAdv.Advertisement.DataSections.Any())
            {
                if (BeaconType == BeaconTypeEnum.Eddystone)
                {
                    ParseEddystoneData(btAdv);
                }
                else if (BeaconType == BeaconTypeEnum.Unknown)
                {
                    // Unknown beacon type
                    //Debug.WriteLine("\nUnknown beacon");
                    //foreach (var dataSection in btAdv.Advertisement.DataSections)
                    //{
                    //    Debug.WriteLine("Data section 0x: " + dataSection.DataType.ToString("X") + " = " + 
                    //        BitConverter.ToString(dataSection.Data.ToArray()));
                    //}
                }
            }

            // Manufacturer data - currently unused
            if (btAdv.Advertisement.ManufacturerData.Any())
            {
                foreach (var manufacturerData in btAdv.Advertisement.ManufacturerData)
                {
                    // Print the company ID + the raw data in hex format
                    //var manufacturerDataString = $"0x{manufacturerData.CompanyId.ToString("X")}: {BitConverter.ToString(manufacturerData.Data.ToArray())}";
                    //Debug.WriteLine("Manufacturer data: " + manufacturerDataString);
                    var manufacturerDataArry = manufacturerData.Data.ToArray();
                    if (manufacturerData.CompanyId == 0x4C && manufacturerData.Data.Length >= 23 &&
                        manufacturerDataArry[0] == 0x02)
                    {
                        BeaconType = BeaconTypeEnum.iBeacon;
                        //Debug.WriteLine("iBeacon Frame: " + BitConverter.ToString(manufacturerDataArry));
                        // Only one relevant data frame for iBeacons
                        if (BeaconFrames.Any())
                        {
                            BeaconFrames[0].Payload = manufacturerDataArry;
                        }
                        else
                        {
                            BeaconFrames.Add(new UnknownBeaconFrame(manufacturerDataArry));
                        }
                    }
                }
            }


        }

        private void ParseEddystoneData(BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            // Parse Eddystone data
            foreach (var dataSection in btAdv.Advertisement.DataSections)
            {
                //Debug.WriteLine("Beacon data: " + dataSection.DataType + " = " +
                //                BitConverter.ToString(dataSection.Data.ToArray()));
                //+ " (" + Encoding.UTF8.GetString(dataSection.Data.ToArray()) + ")\n");
                if (dataSection.DataType == 0x16)
                {
                    var beaconFrame = dataSection.Data.ToArray().CreateBeaconFrame();
                    if (beaconFrame != null)
                    {
                        var found = false;

                        for (var i = 0; i < BeaconFrames.Count; i++)
                        {
                            if (BeaconFrames[i].GetType() == beaconFrame.GetType())
                            {
                                var replaceFrame = false;
                                if (beaconFrame.GetType() == typeof (UnknownBeaconFrame))
                                {
                                    // Unknown frame - also compare eddystone type
                                    var existingEddystoneFrameType =
                                        BeaconFrames[i].Payload.GetEddystoneFrameType();
                                    var newEddystoneFrameType = beaconFrame.Payload.GetEddystoneFrameType();
                                    if (existingEddystoneFrameType != null &&
                                        existingEddystoneFrameType == newEddystoneFrameType)
                                    {
                                        replaceFrame = true;
                                    }
                                }
                                else
                                {
                                    replaceFrame = true;
                                }
                                if (replaceFrame)
                                {
                                    BeaconFrames[i] = beaconFrame;
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (!found)
                        {
                            BeaconFrames.Add(beaconFrame);
                        }
                    }
                }
            }
        }
        
    }
}
