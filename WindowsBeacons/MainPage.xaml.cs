// Copyright 2015 Andreas Jakl, Tieto Corporation. All rights reserved.
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
using System.Diagnostics;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using UniversalBeaconLibrary.Beacon;

namespace WindowsBeacons
{
    public sealed partial class MainPage : Page
    {
        // Bluetooth Beacons
        private readonly BluetoothLEAdvertisementWatcher _watcher;
        private BluetoothLEAdvertisementPublisher _publisher;

        private readonly BeaconManager _beaconManager;

        // UI
        private readonly CoreDispatcher _dispatcher;

        public MainPage()
        {
            this.InitializeComponent();
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            _watcher = new BluetoothLEAdvertisementWatcher();
            _beaconManager = new BeaconManager();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Start watching
            _watcher.Received += WatcherOnReceived;
            _watcher.Start();
            SetStatusOutput("Watching for Bluetooth Beacons");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _watcher.Stop();
            _watcher.Received -= WatcherOnReceived;
        }

        #region Bluetooth Beacons
        
        private void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            _beaconManager.ReceivedAdvertisement(eventArgs);
        }

        #endregion

        #region UI

        private async void SetStatusOutput(string newStatus)
        {
            // Update the status output UI element in the UI thread
            // (some of the callbacks are in a different thread that wouldn't be allowed
            // to modify the UI thread)
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (StatusOutput != null) StatusOutput.Text += "\n" + newStatus; 
            });
        }

        #endregion

    }
}
