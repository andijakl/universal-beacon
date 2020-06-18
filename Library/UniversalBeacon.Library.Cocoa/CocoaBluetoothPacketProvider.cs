using System;
using System.Diagnostics;
using Foundation;
using CoreLocation;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;
using CoreImage;
using System.Linq;

namespace UniversalBeacon.Library
{
    public class CocoaBluetoothPacketProvider : NSObject, IBluetoothPacketProvider
    {
        private readonly string LogTag = nameof(CocoaBluetoothPacketProvider);
        public event EventHandler<BeaconError> WatcherStopped;
        public event EventHandler<BeaconPacketArgs> BeaconRegionEntered;
        public event EventHandler<BeaconPacketArgs> BeaconRegionExited;
        public event EventHandler<BeaconPacketArgs> BeaconReceived;

        private readonly CLLocationManagerDelegate _locationManagerDelegate;
        private readonly CLLocationManager _locationManager;
        private readonly CLBeaconRegion _clBeaconRegion;

        private class UniversalBeaconLocationManagerDelegate : CLLocationManagerDelegate
        {
            private readonly CocoaBluetoothPacketProvider _bluetoothPacketProvider;
            public UniversalBeaconLocationManagerDelegate(CocoaBluetoothPacketProvider bluetoothPacketProvider) : base()
            {
                if(bluetoothPacketProvider is null) { throw new ArgumentNullException(nameof(bluetoothPacketProvider)); }
                _bluetoothPacketProvider = bluetoothPacketProvider;
            }

            public override void DidVisit(CLLocationManager manager, CLVisit visit)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} visited {visit.Coordinate.ToString()}");
            }

            public override void DeferredUpdatesFinished(CLLocationManager manager, NSError error)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} deferred updates {error.DebugDescription}");
            }

            public override void DidRangeBeaconsSatisfyingConstraint(CLLocationManager manager, CLBeacon[] beacons, CLBeaconIdentityConstraint beaconConstraint)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} ranged beacons by constraint");
            }

            public override void Failed(CLLocationManager manager, NSError error)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} something went wrong {error.DebugDescription}");
            }

            public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} locations updated");
            }

            public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} location updated");
            }

            public override void DidStartMonitoringForRegion(CLLocationManager manager, CLRegion region)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} monitoring region {region.Identifier}");
            }

            public override void MonitoringFailed(CLLocationManager manager, CLRegion region, NSError error)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} failed monitoring region {region.Identifier}");
            }

            public override void DidFailRangingBeacons(CLLocationManager manager, CLBeaconIdentityConstraint beaconConstraint, NSError error)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} failed ranging beacons");
            }

            public override void RangingBeaconsDidFailForRegion(CLLocationManager manager, CLBeaconRegion region, NSError error)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} failed ranging beacons for region {region.Identifier}");
            }

            public override void DidDetermineState(CLLocationManager manager, CLRegionState state, CLRegion region)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} region state determined for {region.Identifier} {state}");

                if (state != CLRegionState.Inside || region.Identifier != _bluetoothPacketProvider._clBeaconRegion.Identifier) { return; }

                if (CLLocationManager.IsRangingAvailable)
                {
                    _bluetoothPacketProvider._locationManager.StartRangingBeacons(_bluetoothPacketProvider._clBeaconRegion);
                }
                else
                {
                    Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} Ranging not available");
                }
            }

            public override void RegionEntered(CLLocationManager manager, CLRegion region)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} Region entered {region.Identifier}");

                _bluetoothPacketProvider.BeaconRegionEntered?.Invoke(_bluetoothPacketProvider, new BeaconPacketArgs(
                    new BeaconPacket
                    {
                        Region = new BeaconRegion
                        {
                            RegionName = region.Identifier
                        }
                    }));

                if (CLLocationManager.IsRangingAvailable)
                {
                    _bluetoothPacketProvider._locationManager.StartRangingBeacons(_bluetoothPacketProvider._clBeaconRegion);
                }
                else
                {
                    Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} Ranging not available");
                }
            }

            public override void RegionLeft(CLLocationManager manager, CLRegion region)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} Region left {region.Identifier}");

                _bluetoothPacketProvider.BeaconRegionExited?.Invoke(_bluetoothPacketProvider, new BeaconPacketArgs(
                    new BeaconPacket
                    {
                        Region = new BeaconRegion
                        {
                            RegionName = region.Identifier
                        }
                    }));

                if (CLLocationManager.IsRangingAvailable)
                {
                    _bluetoothPacketProvider._locationManager.StopRangingBeacons(_bluetoothPacketProvider._clBeaconRegion);
                }
                else
                {
                    Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} Ranging not available");
                }
            }

            public override void DidRangeBeacons(CLLocationManager manager, CLBeacon[] beacons, CLBeaconRegion region)
            {
                Debug.WriteLine($"{_bluetoothPacketProvider.LogTag} Beacons ranged");

                _bluetoothPacketProvider.BeaconReceived?.Invoke(_bluetoothPacketProvider, new BeaconPacketArgs(
                    new BeaconPacket
                    {
                        Region = new BeaconRegion
                        {
                            RegionName = region.Identifier
                        }
                    }));
            }
        }

        public CocoaBluetoothPacketProvider(CLBeaconRegion beaconRegion)
        {
            Debug.WriteLine(LogTag);

            if (!CLLocationManager.LocationServicesEnabled)
            {
                Debug.WriteLine($"{LogTag} Location services disabled, my bad.");
                return;
            }

            _locationManagerDelegate = new UniversalBeaconLocationManagerDelegate(this);

            _locationManager = new CLLocationManager();
            _locationManager.Delegate = _locationManagerDelegate;
            //_locationManager.WeakDelegate = _locationManagerDelegate;

            _clBeaconRegion = beaconRegion;

            // DEBUG ONLY, SHOULD START ON ACCEPT
            _locationManager.RequestAlwaysAuthorization();
        }

        public void Start()
        {
            Debug.WriteLine($"{LogTag}:{nameof(Start)}()");

            if (!CLLocationManager.IsMonitoringAvailable(typeof(CLBeaconRegion))){
                Debug.WriteLine($"{LogTag} Cannot monitor beacon regions, my bad.");
                return;
            }

            _locationManager.StartMonitoring(_clBeaconRegion);
        }

        public void Stop()
        {
            Debug.WriteLine($"{LogTag}:{nameof(Stop)}()");


            if (CLLocationManager.IsMonitoringAvailable(typeof(CLBeaconRegion)))
            {
                _locationManager.StopMonitoring(_clBeaconRegion);
            }
            else
            {
                Debug.WriteLine($"{LogTag} Beacon monitor stop failed, monitoring not available.");
            }


            if (CLLocationManager.IsRangingAvailable)
            {
                try
                {
                    _locationManager.StopRangingBeacons(_clBeaconRegion);
                }
                catch (Exception)
                {
                    Debug.WriteLine($"{LogTag} Beacon ranging stop failed. Probably already stopped ranging");
                }
            }

            WatcherStopped?.Invoke(sender: this, e: new BeaconError(BeaconError.BeaconErrorType.Success));
        }
    }
}