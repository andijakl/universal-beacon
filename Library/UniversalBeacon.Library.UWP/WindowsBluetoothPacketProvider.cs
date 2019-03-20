// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
//    http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License. 

using System;
using Windows.Devices.Bluetooth.Advertisement;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.UWP
{
    public class WindowsBluetoothPacketProvider : IBluetoothPacketProvider
    {
        public event EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;
        public event EventHandler<BTError> WatcherStopped;

        private readonly BluetoothLEAdvertisementWatcher _watcher;
        private bool _running;

        public WindowsBluetoothPacketProvider()
        {
            _watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
        }

        /// <summary>
        /// Gets the BluetoothLEAdvertisementWatcher used by the provider instance
        /// </summary>
        public BluetoothLEAdvertisementWatcher AdvertisementWatcher
        {
            get => _watcher;
        }

        public BLEAdvertisementWatcherStatusCodes WatcherStatus
        {
            get
            {
                if (_watcher == null)
                {
                    return BLEAdvertisementWatcherStatusCodes.Stopped;
                }

                return (BLEAdvertisementWatcherStatusCodes)_watcher.Status;
            }
        }

        private void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            AdvertisementPacketReceived?.Invoke(this, new BLEAdvertisementPacketArgs(eventArgs.ToUniversalBLEPacket()));
        }

        public void Start()
        {
            if (_running) return;

            lock (_watcher)
            {
                _watcher.Received += WatcherOnReceived;
                _watcher.Stopped += WatcherOnStopped;
                _watcher.Start();

                _running = true;
            }
        }

        private void WatcherOnStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            WatcherStopped?.Invoke(this, new BTError((BTError.BluetoothError) args.Error));
        }

        public void Stop()
        {
            if (!_running) return;

            lock (_watcher)
            {
                _watcher.Received -= WatcherOnReceived;
                _watcher.Stop();

                _running = false;
            }
        }
    }
}
