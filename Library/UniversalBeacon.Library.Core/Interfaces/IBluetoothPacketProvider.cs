using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{    
    public interface IBluetoothPacketProvider
    {
        event EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;
        void Start();
        void Stop();
    }
}
