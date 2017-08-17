using System;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.Core.Interfaces
{    
    public interface IBluetoothPacketProvider
    {
        event EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;
        void Start();
        void Stop();
    }
}
