using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{    

    public class BLEAdvertisementPacketArgs : EventArgs
    {
        public BLEAdvertisementPacket Data { get; private set; }

        public BLEAdvertisementPacketArgs(BLEAdvertisementPacket data)
        {
            Data = data;
        }
    }
}
