using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{    
    public enum BLEAdvertisementType
    {
        ConnectableUndirected = 0,
        ConnectableDirected = 1,
        ScannableUndirected = 2,
        NonConnectableUndirected = 3,
        ScanResponse = 4
    }
}
