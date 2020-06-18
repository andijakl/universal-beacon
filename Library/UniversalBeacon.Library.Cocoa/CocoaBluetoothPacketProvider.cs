using System;
using System.Diagnostics;
using Foundation;
using CoreLocation;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library
{
    public class CocoaBluetoothPacketProvider : NSObject, IBluetoothPacketProvider
    {
        public event EventHandler<BeaconError> WatcherStopped;
        public event EventHandler<BeaconPacketArgs> BeaconRegionEntered;
        public event EventHandler<BeaconPacketArgs> BeaconRegionExited;

        private readonly CLLocationManagerDelegate _locationManagerDelegate;
        private readonly CLLocationManager _locationManager;
        private readonly CLBeaconRegion _clBeaconRegion;

        private class UniversalBeaconLocationManagerDelegate : CLLocationManagerDelegate
        {
            private readonly CocoaBluetoothPacketProvider _bluetoothPacketProvider;
            public UniversalBeaconLocationManagerDelegate(CocoaBluetoothPacketProvider bluetoothPacketProvider)
            {
                if(bluetoothPacketProvider is null) { throw new ArgumentNullException(nameof(bluetoothPacketProvider)); }
                _bluetoothPacketProvider = bluetoothPacketProvider;
            }
            public override void RegionEntered(CLLocationManager manager, CLRegion region)
            {
                _bluetoothPacketProvider.BeaconRegionEntered?.Invoke(_bluetoothPacketProvider, new BeaconPacketArgs(
                    new BeaconPacket
                    {
                        Region = new BeaconRegion
                        {
                            RegionName = region.Identifier
                        }
                    })
                );
                base.RegionEntered(manager, region);
            }

            public override void RegionLeft(CLLocationManager manager, CLRegion region)
            {
                _bluetoothPacketProvider.BeaconRegionExited?.Invoke(_bluetoothPacketProvider, new BeaconPacketArgs(
                    new BeaconPacket
                    {
                        Region = new BeaconRegion
                        {
                            RegionName = region.Identifier
                        }
                    })
                );
                base.RegionLeft(manager, region);
            }
        }

        public CocoaBluetoothPacketProvider(CLBeaconRegion beaconRegion)
        {
            Debug.WriteLine("BluetoothPacketProvider()");

            _locationManagerDelegate = new CLLocationManagerDelegate();

            _locationManager = new CLLocationManager();
            _locationManager.Delegate = _locationManagerDelegate;

            _clBeaconRegion = beaconRegion;

            // DEBUG ONLY, SHOULD START ON ACCEPT
            _locationManager.RequestAlwaysAuthorization();
        }

        public void Start()
        {
            Debug.WriteLine("BluetoothPacketProvider:Start()");

            _locationManager.StartMonitoring(_clBeaconRegion);
            _locationManager.StartRangingBeacons(_clBeaconRegion);
        }

        public void Stop()
        {
            Debug.WriteLine("BluetoothPacketProvider:Stop()");

            _locationManager.StopMonitoring(_clBeaconRegion);
            _locationManager.StopRangingBeacons(_clBeaconRegion);

            WatcherStopped?.Invoke(sender: this, e: new BeaconError(BeaconError.BeaconErrorType.Success));
        }
    }
}