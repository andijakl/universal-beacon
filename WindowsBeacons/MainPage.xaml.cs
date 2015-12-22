// Copyright 2015 Andreas Jakl, Tieto Corporation. All rights reserved.
// https://github.com/andijakl/universal-beacon 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
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
        private bool _restartingBeaconWatch;

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
            InitializeComponent();
            DataContext = this;
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            // Create the Bluetooth LE watcher from the Windows 10 UWP
            _watcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };

            // Construct the Universal Bluetooth Beacon manager
            _beaconManager = new BeaconManager();
            BeaconListView.ItemsSource = _beaconManager.BluetoothBeacons;

            // Simulate beacon info
//#if DEBUG
//            var eddystoneBeacon = new Beacon(Beacon.BeaconTypeEnum.Eddystone);
//            eddystoneBeacon.BeaconFrames.Add(new TlmEddystoneFrame(0, 3100, (float)25.5, 2000, 1000));
//            eddystoneBeacon.BeaconFrames.Add(new UidEddystoneFrame(220,
//                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A },
//                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 }));
//            eddystoneBeacon.BeaconFrames.Add(new UrlEddystoneFrame(220, "http://www.tieto.at"));
//            eddystoneBeacon.Rssi = -49;
//            eddystoneBeacon.BluetoothAddress = 0x0000e27ef189f6c4; // 3
//            eddystoneBeacon.Timestamp = DateTimeOffset.Now;
//            _beaconManager.BluetoothBeacons.Add(eddystoneBeacon);
//#endif
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _resourceLoader = ResourceLoader.GetForCurrentView();
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            // Start watching
            _watcher.Received += WatcherOnReceived;
            _watcher.Stopped += WatcherOnStopped;
            _watcher.Start();
            if (_watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
            {
                SetStatusOutput(_resourceLoader.GetString("WatchingForBeacons"));
            }
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SetStatusOutput(_resourceLoader.GetString("StoppedWatchingBeacons"));
            _watcher.Stop();
            _watcher.Received -= WatcherOnReceived;
            _restartingBeaconWatch = false;
        }

#region Bluetooth Beacons

        private async void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _beaconManager.ReceivedAdvertisement(eventArgs));
        }

        private void WatcherOnStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            string errorMsg = null;
            if (args != null)
            {
                switch (args.Error)
                {
                    case BluetoothError.Success:
                        errorMsg = "WatchingSuccessfullyStopped";
                        break;
                    case BluetoothError.RadioNotAvailable:
                        errorMsg = "ErrorNoRadioAvailable";
                        break;
                    case BluetoothError.ResourceInUse:
                        errorMsg = "ErrorResourceInUse";
                        break;
                    case BluetoothError.DeviceNotConnected:
                        errorMsg = "ErrorDeviceNotConnected";
                        break;
                    case BluetoothError.DisabledByPolicy:
                        errorMsg = "ErrorDisabledByPolicy";
                        break;
                    case BluetoothError.NotSupported:
                        errorMsg = "ErrorNotSupported";
                        break;
                }
            }
            if (errorMsg == null)
            {
                // All other errors - generic error message
                errorMsg = _restartingBeaconWatch
                    ? "FailedRestartingBluetoothWatch"
                    : "AbortedWatchingBeacons";
            }
            SetStatusOutput(_resourceLoader.GetString(errorMsg));
        }

#if DEBUG
        private void PrintBeaconInfoExample()
        {
            Debug.WriteLine("Beacons discovered so far\n-------------------------");
            foreach (var bluetoothBeacon in _beaconManager.BluetoothBeacons.ToList())
            {
                Debug.WriteLine("\nBeacon: " + bluetoothBeacon.BluetoothAddressAsString);
                Debug.WriteLine("Type: " + bluetoothBeacon.BeaconType);
                Debug.WriteLine("Last Update: " + bluetoothBeacon.Timestamp);
                Debug.WriteLine("RSSI: " + bluetoothBeacon.Rssi);
                foreach (var beaconFrame in bluetoothBeacon.BeaconFrames.ToList())
                {
                    // Print a small sample of the available data parsed by the library
                    if (beaconFrame is UidEddystoneFrame)
                    {
                        Debug.WriteLine("Eddystone UID Frame");
                        Debug.WriteLine("ID: " + ((UidEddystoneFrame) beaconFrame).NamespaceIdAsNumber.ToString("X") + " / " +
                                        ((UidEddystoneFrame) beaconFrame).InstanceIdAsNumber.ToString("X"));
                    }
                    else if (beaconFrame is UrlEddystoneFrame)
                    {
                        Debug.WriteLine("Eddystone URL Frame");
                        Debug.WriteLine("URL: " + ((UrlEddystoneFrame) beaconFrame).CompleteUrl);
                    }
                    else if (beaconFrame is TlmEddystoneFrame)
                    {
                        Debug.WriteLine("Eddystone Telemetry Frame");
                        Debug.WriteLine("Temperature [°C]: " + ((TlmEddystoneFrame) beaconFrame).TemperatureInC);
                        Debug.WriteLine("Battery [mV]: " + ((TlmEddystoneFrame) beaconFrame).BatteryInMilliV);
                    }
                    else
                    {
                        Debug.WriteLine("Unknown frame - not parsed by the library, write your own derived beacon frame type!");
                        Debug.WriteLine("Payload: " + BitConverter.ToString(((UnknownBeaconFrame) beaconFrame).Payload));
                    }
                }
            }
        }
#endif

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


        private void AboutButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void ClearButton_Tapped(object sender, RoutedEventArgs e)
        {
            ClearButton.IsChecked = false;
            _beaconManager?.BluetoothBeacons.Clear();
        }

        private void StatusMsgArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started) return;
            _restartingBeaconWatch = true;
            _watcher.Start();
            if (_watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
            {
                SetStatusOutput(_resourceLoader.GetString("WatchingForBeacons"));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Tools
        /// <summary>
        /// Convert minus-separated hex string to a byte array. Format example: "4E-66-63"
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            // Remove all space characters
            var hexPure = hex.Replace("-", "");
            if (hexPure.Length % 2 != 0)
            {
                // No even length of the string
                throw new Exception("No valid hex string");
            }
            var numberChars = hexPure.Length / 2;
            var bytes = new byte[numberChars];
            var sr = new StringReader(hexPure);
            try
            {
                for (var i = 0; i < numberChars; i++)
                {
                    bytes[i] = Convert.ToByte(new string(new[] { (char)sr.Read(), (char)sr.Read() }), 16);
                }
            }
            catch (Exception)
            {
                throw new Exception("No valid hex string");
            }
            finally
            {
                sr.Dispose();
            }
            return bytes;
        }
        #endregion
    }
}
