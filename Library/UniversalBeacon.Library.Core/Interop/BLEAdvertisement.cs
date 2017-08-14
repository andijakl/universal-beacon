using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{    
    public sealed class BLEAdvertisement
    {
        public string LocalName { get; set; }
        public List<Guid> ServiceUuids { get; private set; }
        public List<BLEAdvertisementDataSection> DataSections { get; private set; }
        public List<BLEManufacturerData> ManufacturerData { get; private set; }

        public BLEAdvertisement()
        {
            ServiceUuids = new List<Guid>();
            DataSections = new List<BLEAdvertisementDataSection>();
            ManufacturerData = new List<BLEManufacturerData>();
        }
    }
}
