using CoreLocation;

namespace UniversalBeacon.Library
{
    public class iOSBluetoothPacketProvider : CocoaBluetoothPacketProvider { 
        public iOSBluetoothPacketProvider(CLBeaconRegion beaconRegion) : base(beaconRegion)
        {

        }
    }
}
