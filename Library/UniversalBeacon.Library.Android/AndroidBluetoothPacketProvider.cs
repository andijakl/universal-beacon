using System;
using System.Collections.Generic;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library
{
    public class AndroidBluetoothPacketProvider : Java.Lang.Object, IBluetoothPacketProvider
    {        
        public event EventHandler<BeaconError> WatcherStopped;
        public event EventHandler<BeaconPacketArgs> BeaconRegionEntered;
        public event EventHandler<BeaconPacketArgs> BeaconRegionExited;

        private readonly BluetoothAdapter _adapter;
        private readonly BLEScanCallback _scanCallback;
        private readonly ScanFilter _scanFilter;

        public AndroidBluetoothPacketProvider(Context context, ScanFilter scanFilter = null)
        {
            var manager = (BluetoothManager)context.GetSystemService("bluetooth");
            _adapter = manager.Adapter;
            _scanCallback = new BLEScanCallback();
            _scanFilter = scanFilter;
        }

        private void ScanCallback_OnAdvertisementPacketReceived(object sender, BeaconPacketArgs e)
        {
            BeaconRegionEntered?.Invoke(this, e);
        }

        public void Start()
        {
            _scanCallback.OnAdvertisementPacketReceived += ScanCallback_OnAdvertisementPacketReceived;
            if (_scanFilter != null)
            {
                _adapter.BluetoothLeScanner.StartScan(new List<ScanFilter> { _scanFilter }, new ScanSettings.Builder().Build(), _scanCallback);
            }
            else
            {
                _adapter.BluetoothLeScanner.StartScan(_scanCallback);
            }
        }

        public void Stop()
        {
            _scanCallback.OnAdvertisementPacketReceived -= ScanCallback_OnAdvertisementPacketReceived;
            _adapter.CancelDiscovery();
        }
    }
}
