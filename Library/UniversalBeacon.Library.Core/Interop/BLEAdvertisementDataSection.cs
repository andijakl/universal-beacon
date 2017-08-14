using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{    

    public sealed class BLEAdvertisementDataSection
    {
        public byte DataType { get; set; }
        public byte[] Data { get; set; }

        public BLEAdvertisementDataSection()
        {
        }
    }
}
