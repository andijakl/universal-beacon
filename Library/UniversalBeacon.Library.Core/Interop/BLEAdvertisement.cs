// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UniversalBeacon.Library.Core.Interop
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
