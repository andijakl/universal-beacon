# Universal Bluetooth Beacon Library

Manage Bluetooth® Beacons through a cross-platform .NET library. Get details for the open Eddystone™ Bluetooth beacon format from Google, as well as beacons comparable to Apple® iBeacon™. Includes ready-to-use Bluetooth scanning for Android, iOS, Mac Catalyst and Windows - works great with .NET MAUI. The platform-independent parsing is available for every platform compatible to .NET Standard 2.0.

Directly use the received Bluetooth Low Energy Advertisement notifications from the base operating system and let the library take care of the rest for you. It extracts, combines and updates unique beacons, associates individual frames to the beacons and parses their contents - e.g., the beacon IDs, URLs or telemetry information like temperature or battery voltage.

[NuGet Library Download](https://www.nuget.org/packages/UniversalBeaconLibrary)



## Background - Bluetooth Beacons

Bluetooth Low Energy / LE (BLE) allows objects to be discovered - for example, it enables your phone to connect to a heart rate belt or a headset. A Bluetooth Beacon does not connect to your phone; instead, it continuously transmits small amounts of data - no matter if someone is listening or not. The efficient nature of Bluetooth ensures that the battery of a beacon nevertheless lasts several months.

Phones and apps can react to Bluetooth Beacons - e.g., to trigger specific actions when the user is close to a physical location. In contrast to GPS, this works even indoors and has a much better accuracy.

To differentiate between different beacons and to give each one a unique meaning in your virtual world, beacons send out information packages. These are formatted according to a certain specification. While the general way of broadcasting these information packages is standardized by the Bluetooth Core specification, the beacon package contents are not. Operating systems come with APIs to receive Bluetooth LE advertisements, but do not contain an SDK to work with common beacon protocols.



## The Universal Beacon Library

Provides an easy way for C#/.NET apps to manage beacons and to parse their information packages.

The library receives Bluetooth advertisements through a platform-specific packet provider - it will analyze, cluster and parse the contents, so that you can easily access the latest data from each individual beacon.

Clustering is achieved through the Bluetooth address (MAC): the constant and regular advertisements of multiple beacons are matched to unique beacons.

The next step is analyzing the contents of the advertisement payloads. The library can work with data comparable to Apple iBeacon (Proximity Beacon) frames, as well as the open [Eddystone specification](https://github.com/google/eddystone), including all frame types that have been defined:

* UID frames
* URL frames
* TLM (Telemetry) frames - unencrypted and encrypted (eTLM)
* EID frames

In addition, the library recognizes advertisements of accessories in Google's [Find Hub Network](https://developers.google.com/nearby/fast-pair/specifications/extensions/fmdn) (e.g., item trackers), which reuse the Eddystone service UUID.

Instead of having to implement the specifications yourself and worry about encodings and byte orderings, you can directly access the latest available information through convenient classes and properties. For unknown frames of other beacon types, it's easy to extend the library to parse the payload in a derived beacon frame class and make use of the beacon management and information update features of the library.

Note: for using Apple iBeacon technology in your services (in order to make your services compatible to iOS devices), you need to agree to and comply with the [Apple iBeacon license agreement](https://developer.apple.com/ibeacon/).



## Feature Overview

- Ready-to-use Bluetooth LE scanning for Android, iOS, Mac Catalyst and Windows
- Clusters based on Bluetooth MAC address and keeps frame types up to date with the latest information
- Retrieve Bluetooth address (MAC), signal strength (RSSI) and latest update timestamp for each beacon
- Thread-safe beacon management, optionally on the UI thread for data binding
- Parses individual advertisement frame contents
- Eddystone UID frame:
  - Ranging data
  - Namespace ID
  - Instance ID
- Eddystone Telemetry (TLM) frame:
  - Version
  - Battery [mV]
  - Beacon temperature [°C]
  - Count of advertisement frames sent since power up
  - Time since power up
- Eddystone encrypted Telemetry (eTLM) frame:
  - Encrypted data, salt and message integrity check
- Eddystone URL frame:
  - Ranging data
  - Complete URL
- Eddystone EID frame:
  - Ranging data
  - Ephemeral Identifier
- Find Hub Network accessory frame:
  - Ephemeral Identifier
  - Unwanted tracking protection mode
- Proximity Beacon Frames (comparable to the Apple iBeacon format)
  - Uuid
  - Major ID
  - Minor ID
  - Tx power
- Raw payload for all other beacons
- Raw advertisement details for all other Bluetooth LE devices:
  - Local name and advertised service UUIDs
  - Manufacturer data (company ID + data) and service data (service UUID + data)
- Parser for raw Bluetooth LE advertising data, e.g., to build your own packet provider



## Status of the Beacon Formats

**Eddystone:** Google has discontinued its services built on top of Eddystone: Nearby Notifications and the Physical Web stopped in December 2018, and the [Eddystone specification repository](https://github.com/google/eddystone) was archived in December 2022. The beacon format itself is final and still widely supported by beacon hardware, so the library continues to support all frame types.

**iBeacon:** the advertisement format has not changed. However, the platforms restrict access:
- **iOS** does not deliver iBeacon advertisements to apps through CoreBluetooth. iBeacons can only be detected through Apple's Core Location APIs, for UUIDs known in advance. Eddystone and other beacons work through this library on iOS.
- **Android 12+** filters some beacons (including iBeacons) from scan results if the app declares the `BLUETOOTH_SCAN` permission with the `neverForLocation` flag. Don't use that flag when you need to detect beacons.
- **Windows** delivers all advertisements.



## Platform Support

| Platform | Target framework | Packet provider | Notes |
|---|---|---|---|
| Android 5.0+ | `net10.0-android` | `AndroidBluetoothPacketProvider` | Requires `BLUETOOTH_SCAN` (Android 12+) and `ACCESS_FINE_LOCATION` permissions |
| iOS 15+ | `net10.0-ios` | `CocoaBluetoothPacketProvider` | Requires `NSBluetoothAlwaysUsageDescription` in Info.plist. No iBeacons (see above). |
| Mac Catalyst 15+ | `net10.0-maccatalyst` | `CocoaBluetoothPacketProvider` | Requires `NSBluetoothAlwaysUsageDescription`, plus the `com.apple.security.device.bluetooth` entitlement when sandboxed |
| Windows 10 1809+ | `net10.0-windows10.0.19041.0` | `WindowsBluetoothPacketProvider` | Works for WinUI 3 / MAUI, WPF, WinForms and console apps. Packaged apps need the `bluetooth` device capability. |
| Any other | `netstandard2.0` | Implement `IBluetoothPacketProvider` | Parsing and beacon management only |

Apple platforms do not expose the Bluetooth MAC address of devices. On iOS and Mac Catalyst, the `BluetoothAddress` is a stable value derived from the system-assigned peripheral identifier.



## Usage Example (C#, .NET MAUI)

Register the platform-specific provider and the beacon manager in `MauiProgram.cs`:

```csharp
#if ANDROID
builder.Services.AddSingleton<IBluetoothPacketProvider>(
    _ => new AndroidBluetoothPacketProvider(Android.App.Application.Context));
#elif IOS || MACCATALYST
builder.Services.AddSingleton<IBluetoothPacketProvider, CocoaBluetoothPacketProvider>();
#elif WINDOWS
builder.Services.AddSingleton<IBluetoothPacketProvider, WindowsBluetoothPacketProvider>();
#endif

// Process received packets on the UI thread, so that the beacon list can be bound to the UI.
builder.Services.AddSingleton(services => new BeaconManager(
    services.GetRequiredService<IBluetoothPacketProvider>(),
    MainThread.BeginInvokeOnMainThread));
```

Then start scanning (after requesting the required permissions) and work with the beacons:

```csharp
_beaconManager.BeaconAdded += (sender, beacon) =>
{
    Debug.WriteLine($"Beacon: {beacon.BluetoothAddressAsString}, type: {beacon.BeaconType}, RSSI: {beacon.Rssi}");
    foreach (var beaconFrame in beacon.BeaconFrames)
    {
        switch (beaconFrame)
        {
            case UidEddystoneFrame uid:
                Debug.WriteLine($"Eddystone UID: {uid.NamespaceIdAsString} / {uid.InstanceIdAsString}");
                break;
            case UrlEddystoneFrame url:
                Debug.WriteLine($"Eddystone URL: {url.CompleteUrl}");
                break;
            case TlmEddystoneFrame tlm:
                Debug.WriteLine($"Eddystone TLM: {tlm.TemperatureInC} °C, {tlm.BatteryInMilliV} mV");
                break;
            case EncryptedTlmEddystoneFrame etlm:
                Debug.WriteLine($"Eddystone eTLM: {etlm.EncryptedDataAsString}");
                break;
            case EidEddystoneFrame eid:
                Debug.WriteLine($"Eddystone EID: {eid.EphemeralIdentifierAsString}");
                break;
            case FindHubNetworkFrame findHub:
                Debug.WriteLine($"Find Hub accessory, unwanted tracking protection: {findHub.IsUnwantedTrackingProtectionModeActive}");
                break;
            case ProximityBeaconFrame proximity:
                Debug.WriteLine($"iBeacon: {proximity.UuidAsString}, major {proximity.Major}, minor {proximity.Minor}");
                break;
            case ManufacturerDataFrame manufacturerData:
                Debug.WriteLine($"Manufacturer data: {manufacturerData.CompanyIdAsString}: {manufacturerData.DataAsString}");
                break;
            case ServiceDataFrame serviceData:
                Debug.WriteLine($"Service data: {serviceData.ServiceUuidAsString}: {serviceData.DataAsString}");
                break;
            default:
                Debug.WriteLine($"Unknown frame: {Convert.ToHexString(beaconFrame.Payload ?? [])}");
                break;
        }
    }
};
_beaconManager.Start();
```

The `BluetoothBeacons` collection of the `BeaconManager`, as well as all beacons and frames, implement change notifications - you can directly bind them to your UI. Use `RemoveStaleBeacons()` to remove beacons that are no longer in range.

The manager collects all Bluetooth LE devices, not only beacons. Devices that don't match a known beacon format have the `BeaconType` `Unknown`; their `LocalName`, `ServiceUuids` as well as `ManufacturerDataFrame` and `ServiceDataFrame` frames give access to the raw advertisement contents. Most phones, laptops and headphones change their Bluetooth address regularly for privacy - remove stale devices regularly if you show all devices.

The provider reports errors - e.g., if Bluetooth is turned off or the user denied access - through its `WatcherStopped` event.

Not using one of the supported platforms? Implement `IBluetoothPacketProvider` yourself. If your platform gives you the raw advertising data, `BLEAdvertisement.Parse()` converts it for the library.



## Example App

The [UniversalBeacon.Sample](samples/UniversalBeacon.Sample) .NET MAUI app runs on Android, iOS, Mac Catalyst and Windows. It continuously scans for Bluetooth LE advertisements, clusters them into beacons and displays the parsed contents of all known frame types. For other devices, it shows the name, service UUIDs, manufacturer data and service data. Use the "Beacons only" switch to hide devices that aren't beacons; devices that haven't been seen for a minute are removed.

Make sure your device has Bluetooth LE capable hardware and that Bluetooth is turned on. On Android, the app asks for the "Nearby devices" and location permissions.

The previous Windows 10 UWP example app ("Bluetooth Beacon Interactor") and the Xamarin samples have been replaced by the .NET MAUI app.



## Building

Requirements: .NET 10 SDK with the MAUI workload (`dotnet workload install maui`). Building the iOS and Mac Catalyst app requires a Mac with Xcode.

```bash
dotnet build src/UniversalBeacon.Library/UniversalBeacon.Library.csproj
dotnet test --project tests/UniversalBeacon.Library.Tests/UniversalBeacon.Library.Tests.csproj
dotnet pack src/UniversalBeacon.Library/UniversalBeacon.Library.csproj -c Release -o artifacts
```

Or open `UniversalBeacon.slnx` in Visual Studio 2026 or Visual Studio Code with the C# Dev Kit.



## Installation

Add the [UniversalBeaconLibrary](https://www.nuget.org/packages/UniversalBeaconLibrary) NuGet package to your project:

```bash
dotnet add package UniversalBeaconLibrary
```



## Version History

### 4.1.0 - 2026
* Raw advertisement details for all devices: `LocalName`, `ServiceUuids`, plus `ManufacturerDataFrame` and `ServiceDataFrame` for data that doesn't belong to a known beacon format. Note: `BeaconFrames` of devices that aren't beacons are therefore no longer empty.
* Eddystone frames are also parsed for beacons that alternate between iBeacon and Eddystone advertisements
* `Beacon.BeaconType` raises change notifications; new `BluetoothUuids` helper to format 16- and 32-bit UUIDs
* iOS / Mac Catalyst: `BLEAdvertisement.ServiceUuids` no longer contains the UUIDs of service data sections, consistent with Android and Windows
* Example app: shows the raw advertisement details, "Beacons only" filter, removes devices that haven't been seen for a minute, keeps the scroll position on Windows

### 4.0.0 - November 2026
* Single NuGet package for Android, iOS, Mac Catalyst and Windows (.NET 10), plus .NET Standard 2.0 for platform-independent parsing
* New iOS and Mac Catalyst support (CoreBluetooth), including Eddystone service data
* Windows support no longer requires UWP: works for WinUI 3 / .NET MAUI, WPF, WinForms and console apps
* Add encrypted Eddystone TLM (eTLM) frames - previously parsed as invalid telemetry data
* Add Find Hub Network accessory frames (Eddystone service UUID, frame types 0x40 / 0x41)
* Unknown Eddystone frame types are now reported as `UnknownBeaconFrame` instead of being dropped
* Malformed frames no longer throw exceptions or report garbage values
* Eddystone beacons are also detected if they only send service data without the service UUID list
* Fix Android: scanning could not be stopped, wrong timestamps, scan errors were ignored, dual-mode devices were skipped
* Fix Windows: restarting after the watcher stopped due to an error
* Fix EID frames created from values had no payload; UID namespace number could be negative
* Thread-safe `BeaconManager` with `RemoveStaleBeacons()`, `Clear()` and `IDisposable`
* New `BLEAdvertisement.Parse()` to parse raw advertising data, including 16-, 32- and 128-bit service UUIDs
* Nullable reference type annotations, unit tests, CI builds, Source Link
* New .NET MAUI example app, replacing the Xamarin and UWP examples
* Breaking changes: assembly renamed to `UniversalBeacon.Library`, `WindowsBluetoothPacketProvider` moved to the `UniversalBeacon.Library` namespace, `ProximityBeaconFrame` no longer throws for invalid payloads, `IsEddystoneFrameType()` now accepts all frame types with the Eddystone header

### 3.2.0 - August 2017
* Add support for Eddystone EID frame type: https://github.com/google/eddystone/tree/master/eddystone-eid
* Additional check for type in IsValid implementation of Eddystone frame types

### 3.1.0 - August 2017
* Improve events provided by cross-platform interop code to surface more events to the custom app implementation. Based on these improvements, the UWP example app now again correctly displays status and error messages.
* Adds BeaconAdded event to the BeaconManager class
* Updated sample code to also include handling Proximity Beacon frames (iBeacon compatible)
* UWP Sample app handles additional Bluetooth error status codes

### 3.0.0 - August 2017
* Port from UWP to .NET Standard 1.3, for cross platform compatibility to Windows, Linux, Mac and Xamarin (iOS, Android)
* Library split into core .NET Standard library, plus platform extension libraries for Android and UWP
* New Xamarin example app for Android and UWP
* Some code changes needed to update from the previous to this version!
* New collaborator: Chris Tacke, https://github.com/ctacke

### 2.0.0 - April 2017
* Add support for beacons comparable to iBeacons (contribution from kobush, https://github.com/andijakl/universal-beacon/pull/4)
* Make Eddystone URLs clickable
* Updated dependencies
* Fix status bar color on Windows 10 Mobile (thanks to Jesse Leskinen for the hint https://twitter.com/jessenic/status/806869124056043521)

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

The Universal Bluetooth Beacon library is classified as stable release.

Any open issues as well as planned features are tracked online:
https://github.com/andijakl/universal-beacon/issues



## License & Related Information

The library is released under the Apache license and is partly based on the [Eddystone™](https://github.com/google/eddystone) specification, which is also released under Apache license - see the LICENSE file for details.
iBeacon™ is a Trademark by Apple Inc. Bluetooth® is a registered trademarks of Bluetooth SIG, Inc.

The example application is licensed under the GPL v3 license - see LICENSE.GPL for details.

Developed by Andreas Jakl, Chris Tacke and Xavier Schott
https://twitter.com/andijakl
https://www.andreasjakl.com/

Library homepage on GitHub:
https://github.com/andijakl/universal-beacon

Library package on NuGet:
https://www.nuget.org/packages/UniversalBeaconLibrary

You might also be interested in the NFC / NDEF library:
https://github.com/andijakl/ndef-nfc
