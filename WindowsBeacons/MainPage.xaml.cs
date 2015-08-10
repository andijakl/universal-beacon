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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using UniversalBeaconLibrary.Annotations;
using UniversalBeaconLibrary.Beacon;

namespace WindowsBeacons
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        // Bluetooth Beacons
        private readonly BluetoothLEAdvertisementWatcher _watcher;
        //private BluetoothLEAdvertisementPublisher _publisher;

        private readonly BeaconManager _beaconManager;

        // UI
        private CoreDispatcher _dispatcher;

        public static readonly DependencyProperty LeftColumnWidthProperty = DependencyProperty.Register(
            "LeftColumnWidth", typeof(int), typeof(MainPage), new PropertyMetadata(default(int)));

        private ResourceLoader _resourceLoader;

        public int LeftColumnWidth
        {
            get { return (int)GetValue(LeftColumnWidthProperty); }
            set { SetValue(LeftColumnWidthProperty, value); }
        }

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (_statusText == value) return;
                _statusText = value;
                OnPropertyChanged();
            }
        }


        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            _watcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };
            _beaconManager = new BeaconManager();
            BeaconListView.ItemsSource = _beaconManager.BluetoothBeacons;

            var x = _watcher.Status;

            // Simulate beacon info
            var eddystoneBeacon = new Beacon(Beacon.BeaconTypeEnum.Eddystone);
            eddystoneBeacon.BeaconFrames.Add(new UrlEddystoneFrame(0, "http://www.tieto.at"));
            _beaconManager.BluetoothBeacons.Add(eddystoneBeacon);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            // Start watching
            _watcher.Received += WatcherOnReceived;
            _watcher.Stopped += WatcherOnStopped;
            _watcher.Start();
            _resourceLoader = ResourceLoader.GetForCurrentView();
            if (_watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
            {
                SetStatusOutput(_resourceLoader.GetString("WatchingForBeacons"));
            }
        }

        private void WatcherOnStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            SetStatusOutput(_resourceLoader.GetString("AbortedWatchingBeacons"));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SetStatusOutput(_resourceLoader.GetString("StoppedWatchingBeacons"));
            _watcher.Stop();
            _watcher.Received -= WatcherOnReceived;
        }

        #region Bluetooth Beacons

        private async void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _beaconManager.ReceivedAdvertisement(eventArgs));
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
                StatusText = newStatus;
            });
        }

        #endregion

        private void AboutButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StatusMsgArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started) return;
            _watcher.Start();
            if (_watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
            {
                SetStatusOutput(_resourceLoader.GetString("WatchingForBeacons"));
            }
        }
    }
}
