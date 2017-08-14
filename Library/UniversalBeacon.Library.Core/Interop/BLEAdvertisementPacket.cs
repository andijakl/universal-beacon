using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
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
