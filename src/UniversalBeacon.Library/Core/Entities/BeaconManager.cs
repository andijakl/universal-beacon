// Copyright 2015 - 2026 Andreas Jakl. All rights reserved.
// https://github.com/andijakl/universal-beacon
//
// Based on the Eddystone specification by Google,
// available under Apache License, Version 2.0 from
// https://github.com/google/eddystone
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// Manages multiple beacons and distributes received Bluetooth LE
/// Advertisements based on unique Bluetooth beacons.
///
/// The manager subscribes to the advertisements of the supplied packet provider.
/// Each advertisement either adds a new Bluetooth beacon to the list
/// of beacons observed so far, or updates a previously known beacon with the
/// new advertisement information.
/// </summary>
public class BeaconManager : IDisposable
{
    /// <summary>
    /// Event that is invoked whenever a new (unknown) beacon is discovered and added to the list
    /// of known beacons (BluetoothBeacons).
    /// To subscribe to updates of every single received Bluetooth advertisement packet,
    /// subscribe to the AdvertisementPacketReceived event of the IBluetoothPacketProvider.
    /// </summary>
    public event EventHandler<Beacon>? BeaconAdded;

    /// <summary>
    /// List of known beacons so far, which all have a unique Bluetooth address
    /// and can have multiple data frames.
    ///
    /// The collection is modified on the thread that handles received advertisements:
    /// supply an invokeAction to the constructor to modify it on the UI thread
    /// if you bind it to the UI.
    /// </summary>
    public ObservableCollection<Beacon> BluetoothBeacons { get; set; } = [];

    /// <summary>
    /// Provider that emits events whenever new Bluetooth advertisement packets have been received.
    /// </summary>
    private readonly IBluetoothPacketProvider _provider;

    /// <summary>
    /// Optional action that is used to handle the received Bluetooth event, e.g., to handle it in a different
    /// thread. Can be used to handle the added beacons on the UI thread, if these are received in a background thread.
    /// </summary>
    private readonly Action<Action>? _invokeAction;

    private readonly object _lock = new();
    private readonly Dictionary<ulong, Beacon> _beaconsByAddress = [];
    private bool _disposed;

    /// <summary>
    /// Create new Beacon Manager based on the provider that is going to update the manager with
    /// new received Bluetooth Packets.
    /// </summary>
    /// <param name="provider">Package provider that emits events whenever Bluetooth advertisement packets
    /// have been received.</param>
    /// <param name="invokeAction">Optional invoke action, e.g., to run the code to handle the received
    /// event on the UI thread.</param>
    public BeaconManager(IBluetoothPacketProvider provider, Action<Action>? invokeAction = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _invokeAction = invokeAction;
        _provider.AdvertisementPacketReceived += OnAdvertisementPacketReceived;
    }

    /// <summary>
    /// Relay the start event to the Bluetooth packet provider.
    /// </summary>
    public void Start()
    {
        _provider.Start();
    }

    /// <summary>
    /// Relay the stop event to the Bluetooth packet provider.
    /// </summary>
    public void Stop()
    {
        _provider.Stop();
    }

    /// <summary>
    /// Remove all beacons that have not sent an advertisement within the specified time span.
    /// </summary>
    /// <param name="maxAge">Maximum time since the last received advertisement.</param>
    /// <returns>Number of removed beacons.</returns>
    public int RemoveStaleBeacons(TimeSpan maxAge)
    {
        var threshold = DateTimeOffset.Now - maxAge;
        var removed = 0;
        lock (_lock)
        {
            for (var i = BluetoothBeacons.Count - 1; i >= 0; i--)
            {
                var beacon = BluetoothBeacons[i];
                if (beacon.Timestamp >= threshold) continue;
                BluetoothBeacons.RemoveAt(i);
                _beaconsByAddress.Remove(beacon.BluetoothAddress);
                removed++;
            }
        }
        return removed;
    }

    /// <summary>
    /// Remove all known beacons.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            BluetoothBeacons.Clear();
            _beaconsByAddress.Clear();
        }
    }

    private void OnAdvertisementPacketReceived(object? sender, BLEAdvertisementPacketArgs e)
    {
        if (_invokeAction != null)
        {
            _invokeAction(() => ReceivedAdvertisement(e.Data));
        }
        else
        {
            ReceivedAdvertisement(e.Data);
        }
    }

    /// <summary>
    /// Analyze the received Bluetooth LE advertisement, and either add a new unique
    /// beacon to the list of known beacons, or update a previously known beacon
    /// with the new information.
    /// </summary>
    /// <param name="btAdv">Bluetooth advertisement to parse.</param>
    private void ReceivedAdvertisement(BLEAdvertisementPacket? btAdv)
    {
        if (btAdv == null || _disposed) return;

        Beacon? newBeacon = null;
        lock (_lock)
        {
            // Rebuild the lookup if the app modified the public collection directly.
            if (_beaconsByAddress.Count != BluetoothBeacons.Count)
            {
                _beaconsByAddress.Clear();
                foreach (var beacon in BluetoothBeacons)
                {
                    _beaconsByAddress[beacon.BluetoothAddress] = beacon;
                }
            }

            if (_beaconsByAddress.TryGetValue(btAdv.BluetoothAddress, out var knownBeacon))
            {
                // We already know this beacon - update / add info to the existing beacon.
                knownBeacon.UpdateBeacon(btAdv);
            }
            else
            {
                // Beacon was not yet known - add it to the list.
                newBeacon = new Beacon(btAdv);
                _beaconsByAddress[btAdv.BluetoothAddress] = newBeacon;
                BluetoothBeacons.Add(newBeacon);
            }
        }

        if (newBeacon != null)
        {
            BeaconAdded?.Invoke(this, newBeacon);
        }
    }

    /// <summary>
    /// Stops listening to the packet provider. Does not dispose the provider.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
        if (disposing)
        {
            _provider.AdvertisementPacketReceived -= OnAdvertisementPacketReceived;
        }
    }
}
