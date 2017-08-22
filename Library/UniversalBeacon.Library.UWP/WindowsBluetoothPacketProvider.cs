using System;
using Windows.Devices.Bluetooth.Advertisement;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.UWP
{
    public class WindowsBluetoothPacketProvider : IBluetoothPacketProvider
    {
        private readonly BluetoothLEAdvertisementWatcher _watcher;

        public event EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;

        public WindowsBluetoothPacketProvider()
        {
            _watcher = new BluetoothLEAdvertisementWatcher
                {
                    ScanningMode = BluetoothLEScanningMode.Active
                };
        }

        private void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            AdvertisementPacketReceived?.Invoke(this, new BLEAdvertisementPacketArgs(eventArgs.ToUniversalBLEPacket()));
        }

        public void Start()
        {
            _watcher.Received += WatcherOnReceived;
            _watcher.Start();
        }

        public void Stop()
        {
            _watcher.Received -= WatcherOnReceived;
            _watcher.Stop();
        }
    }
}
