using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace UniversalBeaconLibrary
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

            _watcher.Received += WatcherOnReceived;
        }

        private void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            AdvertisementPacketReceived?.Invoke(this, new BLEAdvertisementPacketArgs(eventArgs.ToUniversalBLEPacket()));
        }

        public void Start()
        {
            _watcher.Start();
        }

        public void Stop()
        {
            _watcher.Stop();
        }
    }
}
