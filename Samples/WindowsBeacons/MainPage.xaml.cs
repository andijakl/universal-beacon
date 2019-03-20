// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interop;
using UniversalBeacon.Library.UWP;
using UniversalBeaconLibrary;

namespace WindowsBeacons
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private readonly WindowsBluetoothPacketProvider _provider;
        private readonly BeaconManager _beaconManager;

        // UI
        private CoreDispatcher _dispatcher;

        public static readonly DependencyProperty LeftColumnWidthProperty = DependencyProperty.Register(
            "LeftColumnWidth", typeof(int), typeof(MainPage), new PropertyMetadata(default(int)));

        private ResourceLoader _resourceLoader;

        public int LeftColumnWidth
        {
            get => (int)GetValue(LeftColumnWidthProperty);
            set => SetValue(LeftColumnWidthProperty, value);
        }

        private string _statusText;
        private bool _restartingBeaconWatch;

        public string StatusText
        {
            get => _statusText;
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

            // Construct the Universal Bluetooth Beacon manager
            _provider = new WindowsBluetoothPacketProvider();
            _beaconManager = new BeaconManager(_provider, async (action) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { action(); });
            });
            BeaconListView.ItemsSource = _beaconManager.BluetoothBeacons;

            // Subscribe to status change events of the provider
            _provider.WatcherStopped += WatcherOnStopped;
            _beaconManager.BeaconAdded += BeaconManagerOnBeaconAdded;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                // Make sure the status bar is visible also in the light mode on Windows 10 Mobile
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundColor = Color.FromArgb(0, 255, 255, 255);
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundOpacity = 1;
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ForegroundColor = Colors.Black;
            }

            // Simulate beacon info
            //#if DEBUG
            //            var eddystoneBeacon = new Beacon(Beacon.BeaconTypeEnum.Eddystone);
            //            eddystoneBeacon.BeaconFrames.Add(new TlmEddystoneFrame(0, 3100, (float)25.5, 2000, 1000));
            //            // Ranging Data 0xEE = -18dbM: needs unchecked syntax to cast constants, works without unchecked for runtime variables
            //            // (sbyte)0x12 = +18dbM
            //            // Sample values from: https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicViewer.aspx?u=org.bluetooth.characteristic.tx_power_level.xml
            //            eddystoneBeacon.BeaconFrames.Add(new UidEddystoneFrame(unchecked((sbyte)0xEE),
            //                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A },
            //                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 }));
            //            eddystoneBeacon.BeaconFrames.Add(new UrlEddystoneFrame(unchecked((sbyte) 220), "http://www.nfcinteractor.com"));
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
            _beaconManager.Start();

            if (_provider.WatcherStatus == BLEAdvertisementWatcherStatusCodes.Started)
            {
                SetStatusOutput(_resourceLoader.GetString("WatchingForBeacons"));
            }
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SetStatusOutput(_resourceLoader.GetString("StoppedWatchingBeacons"));
            _beaconManager.Stop();
            _restartingBeaconWatch = false;
        }

        #region Bluetooth Beacons
        /// <summary>
        /// Method demonstrating how to handle individual new beacons found by the manager.
        /// This event will only be invoked once, the very first time a beacon is discovered.
        /// For more fine-grained status updates, subscribe to changes of the ObservableCollection in
        /// BeaconManager.BluetoothBeacons (_beaconManager).
        /// To handle all individual received Bluetooth packets in your main app and outside of the
        /// library, subscribe to AdvertisementPacketReceived event of the IBluetoothPacketProvider
        /// (_provider).
        /// </summary>
        /// <param name="sender">Reference to the sender instance of the event.</param>
        /// <param name="beacon">Beacon class instance containing all known and parsed information about
        /// the Bluetooth beacon.</param>
        private void BeaconManagerOnBeaconAdded(object sender, Beacon beacon)
        {
            Debug.WriteLine("\nBeacon: " + beacon.BluetoothAddressAsString);
            Debug.WriteLine("Type: " + beacon.BeaconType);
            Debug.WriteLine("Last Update: " + beacon.Timestamp);
            Debug.WriteLine("RSSI: " + beacon.Rssi);
            foreach (var beaconFrame in beacon.BeaconFrames.ToList())
            {
                // Print a small sample of the available data parsed by the library
                if (beaconFrame is UidEddystoneFrame)
                {
                    Debug.WriteLine("Eddystone UID Frame");
                    Debug.WriteLine("ID: " + ((UidEddystoneFrame)beaconFrame).NamespaceIdAsNumber.ToString("X") + " / " +
                                    ((UidEddystoneFrame)beaconFrame).InstanceIdAsNumber.ToString("X"));
                }
                else if (beaconFrame is UrlEddystoneFrame)
                {
                    Debug.WriteLine("Eddystone URL Frame");
                    Debug.WriteLine("URL: " + ((UrlEddystoneFrame)beaconFrame).CompleteUrl);
                }
                else if (beaconFrame is TlmEddystoneFrame)
                {
                    Debug.WriteLine("Eddystone Telemetry Frame");
                    Debug.WriteLine("Temperature [°C]: " + ((TlmEddystoneFrame)beaconFrame).TemperatureInC);
                    Debug.WriteLine("Battery [mV]: " + ((TlmEddystoneFrame)beaconFrame).BatteryInMilliV);
                }
                else if (beaconFrame is EidEddystoneFrame)
                {
                    Debug.WriteLine("Eddystone EID Frame");
                    Debug.WriteLine("Ranging Data: " + ((EidEddystoneFrame)beaconFrame).RangingData);
                    Debug.WriteLine("Ephemeral Identifier: " + BitConverter.ToString(((EidEddystoneFrame)beaconFrame).EphemeralIdentifier));
                }
                else if (beaconFrame is ProximityBeaconFrame)
                {
                    Debug.WriteLine("Proximity Beacon Frame (iBeacon compatible)");
                    Debug.WriteLine("Uuid: " + ((ProximityBeaconFrame)beaconFrame).UuidAsString);
                    Debug.WriteLine("Major: " + ((ProximityBeaconFrame)beaconFrame).MajorAsString);
                    Debug.WriteLine("Major: " + ((ProximityBeaconFrame)beaconFrame).MinorAsString);
                }
                else
                {
                    Debug.WriteLine("Unknown frame - not parsed by the library, write your own derived beacon frame type!");
                    Debug.WriteLine("Payload: " + BitConverter.ToString(((UnknownBeaconFrame)beaconFrame).Payload));
                }
            }
        }

        private void WatcherOnStopped(object sender, BTError btError)
        {
            string errorMsg = null;
            if (btError != null)
            {
                switch (btError.BluetoothErrorCode)
                {
                    case BTError.BluetoothError.Success:
                        errorMsg = "WatchingSuccessfullyStopped";
                        break;
                    case BTError.BluetoothError.RadioNotAvailable:
                        errorMsg = "ErrorNoRadioAvailable";
                        break;
                    case BTError.BluetoothError.ResourceInUse:
                        errorMsg = "ErrorResourceInUse";
                        break;
                    case BTError.BluetoothError.DeviceNotConnected:
                        errorMsg = "ErrorDeviceNotConnected";
                        break;
                    case BTError.BluetoothError.DisabledByPolicy:
                        errorMsg = "ErrorDisabledByPolicy";
                        break;
                    case BTError.BluetoothError.NotSupported:
                        errorMsg = "ErrorNotSupported";
                        break;
                    case BTError.BluetoothError.OtherError:
                        errorMsg = "ErrorOtherError";
                        break;
                    case BTError.BluetoothError.DisabledByUser:
                        errorMsg = "ErrorDisabledByUser";
                        break;
                    case BTError.BluetoothError.ConsentRequired:
                        errorMsg = "ErrorConsentRequired";
                        break;
                    case BTError.BluetoothError.TransportNotSupported:
                        errorMsg = "ErrorTransportNotSupported";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
                    else if (beaconFrame is EidEddystoneFrame)
                    {
                        Debug.WriteLine("Eddystone EID Frame");
                        Debug.WriteLine("Ranging Data: " + ((EidEddystoneFrame)beaconFrame).RangingData);
                        Debug.WriteLine("Ephemeral Identifier: " + BitConverter.ToString(((EidEddystoneFrame)beaconFrame).EphemeralIdentifier));
                    }
                    else if (beaconFrame is ProximityBeaconFrame)
                    {
                        Debug.WriteLine("Proximity Beacon Frame (iBeacon compatible)");
                        Debug.WriteLine("Uuid: " + ((ProximityBeaconFrame)beaconFrame).UuidAsString);
                        Debug.WriteLine("Major: " + ((ProximityBeaconFrame)beaconFrame).MajorAsString);
                        Debug.WriteLine("Major: " + ((ProximityBeaconFrame)beaconFrame).MinorAsString);
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
            _beaconManager.Start();
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
