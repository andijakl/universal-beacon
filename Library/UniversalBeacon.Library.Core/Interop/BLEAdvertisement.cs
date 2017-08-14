using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{    
    public sealed class BLEAdvertisement
    {
        public string LocalName { get; set; }
        public List<Guid> ServiceUuids { get; set; }
        public List<BLEAdvertisementDataSection> DataSections { get; set; }
        public List<BLEManufacturerData> ManufacturerData { get; set; }

        public BLEAdvertisement()
        {
            ServiceUuids = new List<Guid>();
            DataSections = new List<BLEAdvertisementDataSection>();
            ManufacturerData = new List<BLEManufacturerData>();
        }
    }
}
