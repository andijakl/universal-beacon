// Copyright 2018 - 2026 Xavier Schott, Andreas Jakl. All rights reserved.
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
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library;

/// <summary>
/// Bluetooth LE advertisement provider for iOS and Mac Catalyst, based on CoreBluetooth.
///
/// Your app needs to add the NSBluetoothAlwaysUsageDescription key to its Info.plist.
/// The permission prompt is shown when <see cref="Start"/> is called for the first time.
///
/// Platform limitations:
/// * iOS does not deliver iBeacon advertisements through CoreBluetooth. Detecting iBeacons
///   on iOS requires the Core Location APIs and the beacon UUIDs to be known in advance.
/// * Apple platforms do not expose the Bluetooth MAC address. The BluetoothAddress of received
///   packets is derived from the system-assigned peripheral identifier instead.
/// </summary>
public class CocoaBluetoothPacketProvider : IBluetoothPacketProvider, IDisposable
{
    /// <summary>
    /// iOS reports this value if the RSSI could not be determined.
    /// </summary>
    private const int RssiUnavailable = 127;

    public event EventHandler<BLEAdvertisementPacketArgs>? AdvertisementPacketReceived;
    public event EventHandler<BTError>? WatcherStopped;

    private readonly object _lock = new();
    private CocoaBluetoothCentralDelegate? _centralDelegate;
    private CBCentralManager? _central;
    private bool _scanRequested;
    private bool _isScanning;
    private bool _disposed;

    /// <summary>
    /// True while the scan is running.
    /// </summary>
    public bool IsScanning => _isScanning;

    /// <summary>
    /// Start scanning for Bluetooth LE advertisements.
    /// Scanning starts as soon as Bluetooth is powered on. If Bluetooth is not available or the
    /// user denied access, the WatcherStopped event is raised with the according error code.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CocoaBluetoothPacketProvider));
            _scanRequested = true;
            if (_central == null)
            {
                // Creating the central manager triggers the permission prompt and an
                // initial state update, which starts the scan if Bluetooth is powered on.
                _centralDelegate = new CocoaBluetoothCentralDelegate();
                _centralDelegate.StateUpdated += CentralDelegate_StateUpdated;
                _centralDelegate.PeripheralDiscovered += CentralDelegate_PeripheralDiscovered;
                _central = new CBCentralManager(_centralDelegate, DispatchQueue.MainQueue);
                return;
            }
        }
        HandleState(_central.State);
    }

    /// <summary>
    /// Stop scanning for Bluetooth LE advertisements.
    /// </summary>
    public void Stop()
    {
        bool wasScanning;
        lock (_lock)
        {
            _scanRequested = false;
            wasScanning = _isScanning;
            if (_isScanning && _central != null)
            {
                _central.StopScan();
            }
            _isScanning = false;
        }
        if (wasScanning)
        {
            WatcherStopped?.Invoke(this, new BTError(BTError.BluetoothError.Success));
        }
    }

    private void CentralDelegate_StateUpdated(object? sender, CBManagerState state)
    {
        HandleState(state);
    }

    private void HandleState(CBManagerState state)
    {
        BTError.BluetoothError? error = null;
        lock (_lock)
        {
            if (_central == null) return;
            if (state == CBManagerState.PoweredOn)
            {
                if (_scanRequested && !_isScanning)
                {
                    // Allow duplicates to continuously receive updates (e.g., RSSI, TLM) from beacons.
                    _central.ScanForPeripherals(peripheralUuids: null,
                        options: new PeripheralScanningOptions { AllowDuplicatesKey = true });
                    _isScanning = true;
                }
                return;
            }

            // Bluetooth is not (or no longer) usable.
            var wasActive = _isScanning || _scanRequested;
            _isScanning = false;
            if (!wasActive) return;
            error = state switch
            {
                CBManagerState.PoweredOff => BTError.BluetoothError.RadioNotAvailable,
                CBManagerState.Unauthorized => BTError.BluetoothError.ConsentRequired,
                CBManagerState.Unsupported => BTError.BluetoothError.NotSupported,
                CBManagerState.Resetting => BTError.BluetoothError.ResourceInUse,
                _ => null
            };
        }

        if (error != null)
        {
            WatcherStopped?.Invoke(this, new BTError(error.Value));
        }
    }

    private void CentralDelegate_PeripheralDiscovered(object? sender,
        (CBPeripheral Peripheral, NSDictionary AdvertisementData, NSNumber Rssi) e)
    {
        var rssi = e.Rssi.Int32Value;
        if (rssi == RssiUnavailable) return;

        var packet = new BLEAdvertisementPacket
        {
            BluetoothAddress = ToBluetoothAddress(e.Peripheral.Identifier),
            RawSignalStrengthInDBm = (short)rssi,
            Timestamp = DateTimeOffset.Now,
            AdvertisementType = IsConnectable(e.AdvertisementData)
                ? BLEAdvertisementType.ConnectableUndirected
                : BLEAdvertisementType.NonConnectableUndirected,
            Advertisement = ToAdvertisement(e.Peripheral, e.AdvertisementData)
        };
        AdvertisementPacketReceived?.Invoke(this, new BLEAdvertisementPacketArgs(packet));
    }

    private static bool IsConnectable(NSDictionary advertisementData) =>
        advertisementData[CBAdvertisement.IsConnectable] is NSNumber { BoolValue: true };

    private static BLEAdvertisement ToAdvertisement(CBPeripheral peripheral, NSDictionary advertisementData)
    {
        var advertisement = new BLEAdvertisement
        {
            LocalName = (advertisementData[CBAdvertisement.DataLocalNameKey] as NSString)?.ToString() ?? peripheral.Name
        };

        // Manufacturer data starts with the 16-bit company identifier (little-endian).
        if (advertisementData[CBAdvertisement.DataManufacturerDataKey] is NSData manufacturerData &&
            manufacturerData.Length >= 2)
        {
            advertisement.ManufacturerData.Add(
                new BLEManufacturerData(BLEPacketType.ManufacturerData, manufacturerData.ToArray()));
        }

        // Advertised service UUIDs
        if (advertisementData[CBAdvertisement.DataServiceUUIDsKey] is NSArray serviceUuids)
        {
            for (nuint i = 0; i < serviceUuids.Count; i++)
            {
                var uuid = serviceUuids.GetItem<CBUUID>(i);
                var guid = ToGuid(uuid);
                if (guid != null) advertisement.ServiceUuids.Add(guid.Value);
            }
        }

        // Service data - CoreBluetooth separates the service UUID from the data.
        // Reconstruct the on-air format: [service UUID, little-endian] [data].
        if (advertisementData[CBAdvertisement.DataServiceDataKey] is NSDictionary serviceData)
        {
            foreach (var entry in serviceData)
            {
                if (entry.Key is not CBUUID uuid || entry.Value is not NSData data) continue;
                var uuidBytes = uuid.Data.ToArray();
                Array.Reverse(uuidBytes);
                var dataBytes = data.ToArray();
                var sectionData = new byte[uuidBytes.Length + dataBytes.Length];
                Buffer.BlockCopy(uuidBytes, 0, sectionData, 0, uuidBytes.Length);
                Buffer.BlockCopy(dataBytes, 0, sectionData, uuidBytes.Length, dataBytes.Length);

                var packetType = uuidBytes.Length switch
                {
                    2 => BLEPacketType.ServiceData,
                    4 => BLEPacketType.ServiceData32,
                    _ => BLEPacketType.ServiceData128
                };
                advertisement.DataSections.Add(new BLEAdvertisementDataSection(packetType, sectionData));

                // CoreBluetooth only lists the UUID in the service data, not necessarily in the UUID list.
                var guid = ToGuid(uuid);
                if (guid != null && !advertisement.ServiceUuids.Contains(guid.Value))
                {
                    advertisement.ServiceUuids.Add(guid.Value);
                }
            }
        }

        return advertisement;
    }

    private static Guid? ToGuid(CBUUID uuid)
    {
        var bytes = uuid.Data.ToArray();
        var expanded = bytes.Length switch
        {
            2 => "0000" + Convert.ToHexString(bytes) + "-0000-1000-8000-00805F9B34FB",
            4 => Convert.ToHexString(bytes) + "-0000-1000-8000-00805F9B34FB",
            16 => Convert.ToHexString(bytes),
            _ => null
        };
        return expanded != null && Guid.TryParse(expanded, out var guid) ? guid : null;
    }

    /// <summary>
    /// Derive a stable 48-bit pseudo address from the peripheral identifier,
    /// as Apple platforms do not expose the Bluetooth MAC address.
    /// </summary>
    private static ulong ToBluetoothAddress(NSUuid identifier)
    {
        var bytes = identifier.GetBytes();
        ulong address = 0;
        for (var i = 0; i < bytes.Length; i++)
        {
            // Fold all 16 bytes into 6 bytes
            address ^= (ulong)bytes[i] << ((i % 6) * 8);
        }
        return address & 0xFFFFFFFFFFFF;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            Stop();
            lock (_lock)
            {
                if (_centralDelegate != null)
                {
                    _centralDelegate.StateUpdated -= CentralDelegate_StateUpdated;
                    _centralDelegate.PeripheralDiscovered -= CentralDelegate_PeripheralDiscovered;
                }
                _central?.Dispose();
                _centralDelegate?.Dispose();
                _central = null;
                _centralDelegate = null;
            }
        }
        _disposed = true;
    }
}
