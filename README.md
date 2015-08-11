# Universal Beacon Library for Windows 10
========

Support for Bluetooth Beacons for the Universal Windows Platform (Windows 10), including the Eddystone specification.

Directly use the received Bluetooth LE Advertisement notifications from the Windows 10 base system and let the library take care of the rest for you. It extracts, combines and updates unique beacons, associated individual frames to the beacons and parses their contents - e.g., the beacon IDs, URLs or telemetry information like temperature or battery voltage.



## Background - Bluetooth Beacons

Bluetooth Low Energy / LE / Smart allows objects to be discovered - for example, it enables your phone to connect to a heart rate belt or a headset. A Bluetooth Beacon does not connect to your phone; instead, it continuously transmits small amounts of data. 



## The Universal Beacon Library

... coming soon!



## Availability

The Universal Beacon Library is available in C# and includes a dependency to .NETCore the Universal Windows Platform (UWP / UAP) for Windows 10, and can therefore be used in applications targeting Windows 10 with support for Bluetooth Smart / LE.

To keep up to date, either watch this project or [follow me on Twitter](https://twitter.com/andijakl).



## Feature Overview

... coming soon!



## Windows 10 Example App

The included Windows 10 example app continuously scans for Bluetooth LE advertisements. It associates these with known or new Bluetooth MAC addresses to identify beacons. The individual advertisement frames are then parsed for known frame types - which are currently the three frame types defined by the Google Eddystone specification.

The app has been tested on Windows 10 tablets and phones and requires Bluetooth LE / Smart capable hardware. Make sure your device has Bluetooth activated (in Windows settings and also in hardware in case your device allows turning off bluetooth using a key combination) and is not in airplane mode.



## Usage example (C#)

### Registering for beacons and handling the data

``` 
public sealed partial class MainPage : Page, INotifyPropertyChanged
{
	// Bluetooth Beacons
	private readonly BluetoothLEAdvertisementWatcher _watcher;
	//private BluetoothLEAdvertisementPublisher _publisher;

	private readonly BeaconManager _beaconManager;
	
	public MainPage()
	{
		// [...]
		// Construct the Universal Bluetooth Beacon manager
		_beaconManager = new BeaconManager();
		
		// Create & start the Bluetooth LE watcher from the Windows 10 UWP
		_watcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };
		_watcher.Received += WatcherOnReceived;
		_watcher.Start();
	}
	
	private async void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
	{
		// Let the library manager handle the advertisement to analyse & store the advertisement
		_beaconManager.ReceivedAdvertisement(eventArgs);
	}
	
	// Call this method e.g., when tapping a button
	private void PrintBeaconInfo()
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
}
		
			
``` 



## Installation

If you want to use the Universal Beacon Library from your own app, the easiest option is to use the NuGet package manager in Visual Studio 2015 to automatically download & integrate the library:

1. Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...
2. Search "Online" for "Universal Bluetooth Beacon Library"
3. Install the library.

To try the Windows 10 example app, download the complete library package from this site.



## Version History

### 1.0.0.0 - August 2015
* Initial release
* Works with received Bluetooth advertisements from the Windows 10 Bluetooth LE API
* Combines individual received frames based on the Bluetooth MAC address to associate them with unique beacons
* Support for parsing Google Eddystone URL, UID and TLM frames



## Status & Roadmap

The Universal Bluetooth Beacon library is classified as stable release and is in use in several projects, most importantly Bluetooth Beacon Interactor for Windows 10.

Any open issues as well as planned features are tracked online:
https://github.com/andijakl/universal-beacon/issues



## License & Related Information

The library is released under the Apache license and is partly based on the Google Eddystone specification, which is also released under Apache license https://github.com/google/eddystone - see the LICENSE file for details.

Developed by Andreas Jakl, Tieto Corporation
https://twitter.com/andijakl
http://www.tieto.com/

Library homepage on GitHub:
https://github.com/andijakl/universal-beacon

You might also be interested in the NFC / NDEF library:
https://github.com/andijakl/ndef-nfc

