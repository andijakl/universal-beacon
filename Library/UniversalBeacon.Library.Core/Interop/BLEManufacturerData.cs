using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{    
    public sealed class BLEManufacturerData
    {
        public byte[] Data { get; set; }
        public ushort CompanyId { get; set; }

        public BLEManufacturerData()
        {
        }
    }
}
