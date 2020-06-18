using System;
using System.Collections.Generic;
using System.Diagnostics;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library
{
    public class AndroidBluetoothPacketProvider : Java.Lang.Object, IBluetoothPacketProvider
    {
        private readonly string LogTag = nameof(AndroidBluetoothPacketProvider);

        public event EventHandler<BeaconError> WatcherStopped;
        public event EventHandler<BeaconPacketArgs> BeaconRegionEntered;
        public event EventHandler<BeaconPacketArgs> BeaconRegionExited;
        public event EventHandler<BeaconPacketArgs> BeaconReceived;

        private readonly BluetoothAdapter _adapter;
        private readonly BLEScanCallback _scanCallback;
        private readonly ScanFilter _scanFilter;

        public AndroidBluetoothPacketProvider(Context context, ScanFilter scanFilter = null)
        {
            Debug.WriteLine(LogTag);

            var manager = (BluetoothManager)context.GetSystemService("bluetooth");
            _adapter = manager.Adapter;
            _scanCallback = new BLEScanCallback();
            _scanFilter = scanFilter;
        }

        private void ScanCallback_OnAdvertisementPacketReceived(object sender, BeaconPacketArgs e)
        {
            BeaconReceived?.Invoke(this, e);
        }

        public void Start()
        {
            Debug.WriteLine($"{LogTag}:{nameof(Start)}()");

            if (_adapter.BluetoothLeScanner is null)
            {
                Debug.WriteLine($"{LogTag} adapter is null, please turn bluetooth on");
                return;
            }

            _scanCallback.OnAdvertisementPacketReceived += ScanCallback_OnAdvertisementPacketReceived;

            var scanSettings = new ScanSettings.Builder().SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower).Build();

            if (_scanFilter != null)
            {
                Debug.WriteLine($"{LogTag} starting filtered scan");
                _adapter.BluetoothLeScanner.StartScan(new List<ScanFilter> { _scanFilter }, scanSettings, _scanCallback);
            }
            else
            {
                Debug.WriteLine($"{LogTag} starting unfiltered scan");
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
