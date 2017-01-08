# Universal Beacon Library for Windows 10

Support for Bluetooth® Beacons for the Universal Windows Platform (Windows 10), including the Eddystone™ Smart beacon format.

Directly use the received Bluetooth Smart Advertisement notifications from the Windows 10 base system and let the library take care of the rest for you. It extracts, combines and updates unique beacons, associated individual frames to the beacons and parses their contents - e.g., the beacon IDs, URLs or telemetry information like temperature or battery voltage.

[NuGet Library Download](https://www.nuget.org/packages/UniversalBeaconLibrary) | [Windows 10 Example App Download](https://www.microsoft.com/store/apps/9NBLGGH1Z24K)



## Background - Bluetooth Beacons

Bluetooth Low Energy / LE / Smart allows objects to be discovered - for example, it enables your phone to connect to a heart rate belt or a headset. A Bluetooth Beacon does not connect to your phone; instead, it continuously transmits small amounts of data - no matter if someone is listening or not. The efficient nature of Bluetooth Smart ensures that the battery of a beacon nevertheless lasts several months.

Phones and apps can react to Bluetooth Beacons - e.g., to trigger specific actions when the user is close to a physical location. In contrast to GPS, this works even indoors and has a much better accuracy.

To differentiate between different beacons and to give each one a unique meaning in your virtual world, beacons send out information packages. These are formatted according to a certain specification. While the general way of broadcasting these information packages is standardized by the Bluetooth Core specification, the beacon package contents are not. Windows 10 comes with APIs to receive Bluetooth LE advertisements, but does not contain an SDK to work with common beacon protocols.



## The Universal Beacon Library

Provides an easy way for Universal Windows Apps (for Windows 10) to manage beacons and to parse their information packages.

As a developer, you only have to feed the received Bluetooth advertisements into the library - it will analyze, cluster and parse the contents, so that you can easily access the latest data from each individual beacon.

Clustering is achieved through the Bluetooth address (MAC): the constant and regular advertisements of multiple beacons are matched to unique beacons.

The next step is analyzing the conents of the advertisement payloads. The library recognizes Apple iBeacons and fully parses the open [Eddystone specification](https://github.com/google/eddystone), including all three frame types that have been defined:

* UID frames
* URL frames
* Telemetry frames

Instead of having to implement the Eddystone specification yourself and worry about encodings and byte orderings, you can directly access the latest available information through convenient classes and properties. For unknown frames of other beacon types, it's easy to extend the library to parse the payload in a derived beacon frame class and make use of the beacon management and information update features of the library.



## Feature Overview

- Directly analyzes received Bluetooth LE advertisements (`BluetoothLEAdvertisementReceivedEventArgs`)
- Clusters based on Bluetooth MAC address and keeps frame types up to date with the latest information
- Retrieve Bluetooth address (MAC), signal strength (RSSI) and latest update timestamp for each beacon
- Parses individual advertisement frame contents
- Eddystone UID frame:
  - Ranging data
  - Namespace ID
  - Instance ID
- Eddystone Telemetry (TLM) frame:
  - Version
  - Battery [V]
  - Beacon temperature [°C]
  - Count of advertisement frames sent since power up
  - Time since power up
- Eddystone URL frame:
  - Ranging data
  - Complete URL
- Raw payload for all other beacons, including Apple iBeacon



## Windows 10 Example App

The included Windows 10 example app continuously scans for Bluetooth LE advertisements. It associates these with known or new Bluetooth MAC addresses to identify beacons. The individual advertisement frames are then parsed for known frame types - which are currently the three frame types defined by the Google Eddystone specification.

The app has been tested on Windows 10 tablets and phones and requires Bluetooth LE / Smart capable hardware. Make sure your device has Bluetooth activated (in Windows settings and also in hardware in case your device allows turning off bluetooth using a key combination) and is not in airplane mode.

Download and test the example app from the Windows 10 store: https://www.microsoft.com/store/apps/9NBLGGH1Z24K


### Permissions and Privacy Settings in Windows 10

To allow apps to receive data from Bluetooth Beacons, you have to ensure Windows 10 is configured correctly.

1. Turn off flight mode: Settings -> Network & Internet -> Flight mode
2. Turn on Bluetooth: Settings -> Devices -> Bluetooth
3. Turn on Device Sync: Settings -> Privacy -> Other devices -> Sync with devices (Example: beacons).



## Usage example (C#)

### Registering for beacons and handling the data

```csharp
public sealed partial class MainPage : Page
{
	// Bluetooth Beacons
	private readonly BluetoothLEAdvertisementWatcher _watcher;

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



## Availability

The Universal Beacon Library is available in C# and includes a dependency to .NETCore the Universal Windows Platform (UWP / UAP) for Windows 10 to directly work with received advertisement packets from the Windows Bluetooth API. The library can therefore be used in applications targeting Windows 10 with support for Bluetooth Smart / LE.

To keep up to date, either watch this project or [follow me on Twitter](https://twitter.com/andijakl).




## Installation

If you want to use the Universal Beacon Library from your own app, the easiest option is to use the NuGet package manager in Visual Studio 2015 to automatically download & integrate the library:

1. Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...
2. Search "Online" for "Universal Bluetooth Beacon Library"
3. Install the library.

Alternatively, use the NuGet Package Manager console as described here: https://www.nuget.org/packages/UniversalBeaconLibrary

To try the Windows 10 example app, download the complete library package from this site.



## Version History

### 1.8.1 - February 2016
* Use sbyte instead of byte for accessing ranging data in Eddystone UID and URL frames to ease development and remove the need for manual casting.

### 1.7.0 - January 2016
* Added translations to Chinese, French, Russian and Portuguese

### 1.6.0 - January 2016
* Added translation to German
* Fix crash when a Bluetooth Beacon with no further data is found.

### 1.5.0 - December 2015
* Allow last two bytes of the Eddystone UID frame to be 0x00 0x00 for RFU, according to the specification.
Some beacons do not send these bytes; the library now allows both variants. When creating a UID record,
the library will now add the two additional bytes to conform to the spec.

### 1.4.0 - December 2015
* Fix black window background color in example app on latest Windows 10 Mobile version

### 1.3.0 - September 2015
* Example app allows clearing visible information

### 1.2.0 - August 2015
* Improvements in naming and Eddystone references

### 1.1.0 - August 2015
* Manually construct Eddystone TLM & UID frames

### 1.0.0 - August 2015
* Initial release
* Works with received Bluetooth advertisements from the Windows 10 Bluetooth LE API
* Combines individual received frames based on the Bluetooth MAC address to associate them with unique beacons
* Support for parsing Google Eddystone URL, UID and TLM frames



## Status & Roadmap

The Universal Bluetooth Beacon library is classified as stable release and is in use in several projects, most importantly [Bluetooth Beacon Interactor](https://www.microsoft.com/store/apps/9NBLGGH1Z24K) for Windows 10.

Any open issues as well as planned features are tracked online:
https://github.com/andijakl/universal-beacon/issues



## License & Related Information

The library is released under the Apache license and is partly based on the [Eddystone™](https://github.com/google/eddystone) specification, which is also released under Apache license - see the LICENSE file for details.
iBeacon™ is a Trademark by Apple Inc. Bluetooth® and Bluetooth Smart are registered trademarks of Bluetooth SIG, Inc.

The example application is licensed under the GPL v3 license - see LICENSE.GPL for details.

Developed by Andreas Jakl
https://twitter.com/andijakl

Library homepage on GitHub:
https://github.com/andijakl/universal-beacon

Library package on NuGet:
https://www.nuget.org/packages/UniversalBeaconLibrary

You might also be interested in the NFC / NDEF library:
https://github.com/andijakl/ndef-nfc

