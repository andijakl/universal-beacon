// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;

namespace UniversalBeacon.Library.Core.Interop
{    
    public class BLEAdvertisementPacket
    {
        public BLEAdvertisement Advertisement { get; set; }
        public BLEAdvertisementType AdvertisementType { get; set; }

        public ulong BluetoothAddress { get; set; }
        public short RawSignalStrengthInDBm { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
