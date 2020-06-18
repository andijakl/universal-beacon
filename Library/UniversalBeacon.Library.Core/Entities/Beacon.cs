// Copyright 2015 - 2017 Andreas Jakl. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// Based on the Eddystone specification by Google, 
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.Core.Entities
{
    /// <summary>
    /// Represents a single unique beacon that has a specified Bluetooth MAC address.
    /// Construction and updates are usually handled by the BeaconManager.
    /// 
    /// Construct this class based on a Bluetooth advertisement received from the
    /// Windows Bluetooth API. When further advertisements are received for this beacon,
    /// call its UpdateBeacon() method to update the frames.
    /// </summary>
    public class Beacon : INotifyPropertyChanged
    {
        /// <summary>
        /// Bluetooth Service UUID for Eddystone beacons.
        /// </summary>
        private readonly Guid _eddystoneGuid = new Guid("0000FEAA-0000-1000-8000-00805F9B34FB");

        public enum BeaconTypeEnum
        {
            /// <summary>
            /// Bluetooth LE advertisment that is not recognized as one of the beacon formats
            /// supported by this library.
            /// </summary>
            Unknown,
            /// <summary>
            /// Beacon conforming to the Eddystone specification by Google.
            /// </summary>
            Eddystone,
            /// <summary>
            /// Beacon conforming to the Apple iBeacon specification.
            /// iBeacon is a Trademark of Apple Inc.
            /// Note: the beacon broadcast payload is not parsed by this library.
            /// </summary>
            iBeacon
        }

        /// <summary>
        /// Type of this beacon.
        /// Defines how the beacon will parse the individual frames to extract information from the
        /// advertisements.
        /// </summary>
        public BeaconTypeEnum BeaconType { get; set; } = BeaconTypeEnum.Unknown;

        /// <summary>
        /// List of all the different frames that have been observed for this beacon so far.
        /// If a new frame with the same type is collected, it replaces the previous frame.
        /// </summary>
        public ObservableCollection<BeaconFrameBase> BeaconFrames { get; set; } = new ObservableCollection<BeaconFrameBase>();

        private short _rssi;
        /// <summary>
        /// Raw signal strength in dBM.
        /// If a new advertisement is received for the same beacon (with the same
        /// Bluetooth MAC address), always the latest signal strength is recorded.
        /// </summary>
        public short Rssi
        {
            get => _rssi;
            set
            {
                if (_rssi == value) return;
                _rssi = value;
                OnPropertyChanged();
            }
        }


        private BeaconRegion _region;
        /// <summary>
        /// Raw signal strength in dBM.
        /// If a new advertisement is received for the same beacon (with the same
        /// Bluetooth MAC address), always the latest signal strength is recorded.
        /// </summary>
        public BeaconRegion Region
        {
            get => _region;
            set
            {
                if (_region == value) return;
                _region = value;
                OnPropertyChanged();
            }
        }

        private ulong _bluetoothAddress;
        /// <summary>
        /// The Bluetooth MAC address.
        /// Used to cluster the different received Bluetooth advertisements and to
        /// collect multiple advertisements for unique beacons.
        /// </summary>
        public ulong BluetoothAddress
        {
            get => _bluetoothAddress;
            set
            {
                if (_bluetoothAddress == value) return;
                _bluetoothAddress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BluetoothAddressAsString));
            }
        }

        /// <summary>
        /// Retrieves the Bluetooth MAC address formatted as a hex string.
        /// </summary>
        public string BluetoothAddressAsString
        {
            get
            {
                return string.Join(":", BitConverter.GetBytes(BluetoothAddress).Reverse().Select(b => b.ToString("X2"))).Substring(6);
            }
        }

        private DateTimeOffset _timestamp;
        /// <summary>
        /// Timestamp when the last advertisement was received for this beacon.
        /// </summary>
        public DateTimeOffset Timestamp
        {
            get => _timestamp;
            set
            {
                if (_timestamp == value) return;
                _timestamp = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Construct a new Bluetooth beacon based on the received advertisement.
        /// Tries to find out if it's a known type, and then parses the contents accordingly.
        /// </summary>
        /// <param name="btAdv">Bluetooth advertisement to parse, as received from
        /// the Windows Bluetooth LE API.</param>
        public Beacon(BeaconPacket btAdv)
        {
            BluetoothAddress = btAdv.BluetoothAddress;
            Rssi = btAdv.RawSignalStrengthInDBm;
            Timestamp = btAdv.Timestamp;
            Region = btAdv.Region;
        }

        /// <summary>
        /// Manually create a new Beacon instance.
        /// </summary>
        /// <param name="beaconType">Beacon type to use for this manually constructed beacon.</param>
        public Beacon(BeaconTypeEnum beaconType)
        {
            BeaconType = beaconType;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
