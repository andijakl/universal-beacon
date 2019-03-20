// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
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

using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.UWP
{
    internal static class PacketExtensions
    {
        /// <summary>
        /// Convert the Windows specific Bluetooth LE advertisment data to the universal cross-platform structure
        /// used by the Universal Beacon Library.
        /// </summary>
        /// <param name="args">Windows Bluetooth LE advertisment data to convert to cross-platform format.</param>
        /// <returns>Packet data converted to cross-platform format.</returns>
        public static BLEAdvertisementPacket ToUniversalBLEPacket(this BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var packet = new BLEAdvertisementPacket
            {
                Timestamp = args.Timestamp,
                BluetoothAddress = args.BluetoothAddress,
                RawSignalStrengthInDBm = args.RawSignalStrengthInDBm,
                AdvertisementType = (BLEAdvertisementType) args.AdvertisementType,
                Advertisement = args.Advertisement.ToUniversalAdvertisement()
            };

            return packet;
        }

        /// <summary>
        /// Convert the Windows specific Bluetooth LE advertisment to the universal cross-platform structure
        /// used by the Universal Beacon Library.
        /// </summary>
        /// <param name="convertBleAdvertisment">Windows Bluetooth LE advertisment to convert to cross-platform format.</param>
        /// <returns>Packet converted to cross-platform format.</returns>
        public static BLEAdvertisement ToUniversalAdvertisement(this BluetoothLEAdvertisement convertBleAdvertisment)
        {
            if (convertBleAdvertisment == null) return null;

            var result = new BLEAdvertisement
            {
                LocalName = convertBleAdvertisment.LocalName
            };

            result.ServiceUuids.AddRange(convertBleAdvertisment.ServiceUuids);

            if (convertBleAdvertisment.DataSections != null)
            {
                foreach (var curDataSection in convertBleAdvertisment.DataSections)
                {
                    var data = new BLEAdvertisementDataSection
                    {
                        DataType = curDataSection.DataType,
                        Data = curDataSection.Data.ToArray()
                    };

                    result.DataSections.Add(data);
                }
            }

            if (convertBleAdvertisment.ManufacturerData != null)
            {
                foreach (var curManufacturerData in convertBleAdvertisment.ManufacturerData)
                {
                    var data = new BLEManufacturerData
                    {
                        CompanyId = curManufacturerData.CompanyId,
                        Data = curManufacturerData.Data.ToArray()
                    };

                    result.ManufacturerData.Add(data);
                }
            }

            return result;
        }
    }
}
