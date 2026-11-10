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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// Represents a single unique beacon that has a specified Bluetooth address.
/// Construction and updates are usually handled by the BeaconManager.
///
/// Construct this class based on a received Bluetooth advertisement.
/// When further advertisements are received for this beacon,
/// call its UpdateBeacon() method to update the frames.
/// </summary>
public class Beacon : INotifyPropertyChanged
{
    /// <summary>
    /// Bluetooth Service UUID for Eddystone beacons.
    /// </summary>
    private static readonly Guid EddystoneGuid = new("0000FEAA-0000-1000-8000-00805F9B34FB");

    /// <summary>
    /// AD type of 16-bit UUID service data sections.
    /// </summary>
    private const byte ServiceData16BitUuidType = 0x16;

    public enum BeaconTypeEnum
    {
        /// <summary>
        /// Bluetooth LE advertisement that is not recognized as one of the beacon formats
        /// supported by this library.
        /// </summary>
        Unknown,

        /// <summary>
        /// Beacon conforming to the Eddystone specification by Google,
        /// or another format using the Eddystone service UUID (e.g., Find Hub Network accessories).
        /// </summary>
        Eddystone,

        /// <summary>
        /// Beacon conforming to the Apple iBeacon specification.
        /// iBeacon is a Trademark of Apple Inc.
        /// </summary>
        iBeacon
    }

    /// <summary>
    /// Type of this beacon.
    /// Defines how the beacon will parse the individual frames to extract information from the
    /// advertisements.
    /// </summary>
    public BeaconTypeEnum BeaconType { get; set; } = BeaconTypeEnum.Unknown;

    /// <summary>
    /// List of all the different frames that have been observed for this beacon so far.
    /// If a new frame with the same type is collected, it replaces the previous frame.
    /// </summary>
    public ObservableCollection<BeaconFrameBase> BeaconFrames { get; set; } = [];

    private short _rssi;

    /// <summary>
    /// Raw signal strength in dBm.
    /// If a new advertisement is received for the same beacon (with the same
    /// Bluetooth address), always the latest signal strength is recorded.
    /// </summary>
    public short Rssi
    {
        get => _rssi;
        set
        {
            if (_rssi == value) return;
            _rssi = value;
            OnPropertyChanged();
        }
    }

    private ulong _bluetoothAddress;

    /// <summary>
    /// The Bluetooth address.
    /// Used to cluster the different received Bluetooth advertisements and to
    /// collect multiple advertisements for unique beacons.
    ///
    /// Note: iOS and macOS do not expose the Bluetooth MAC address. On these platforms,
    /// the address is derived from the system-assigned peripheral identifier instead.
    /// </summary>
    public ulong BluetoothAddress
    {
        get => _bluetoothAddress;
        set
        {
            if (_bluetoothAddress == value) return;
            _bluetoothAddress = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BluetoothAddressAsString));
        }
    }

    /// <summary>
    /// Retrieves the 48-bit Bluetooth address formatted as hex string (e.g., "C4:7F:51:0A:12:34").
    /// </summary>
    public string BluetoothAddressAsString
    {
        get
        {
            var sb = new StringBuilder(17);
            for (var shift = 40; shift >= 0; shift -= 8)
            {
                if (sb.Length > 0) sb.Append(':');
                sb.Append(((byte)(BluetoothAddress >> shift)).ToString("X2"));
            }
            return sb.ToString();
        }
    }

    private DateTimeOffset _timestamp;

    /// <summary>
    /// Timestamp when the last advertisement was received for this beacon.
    /// </summary>
    public DateTimeOffset Timestamp
    {
        get => _timestamp;
        set
        {
            if (_timestamp == value) return;
            _timestamp = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Construct a new Bluetooth beacon based on the received advertisement.
    /// Tries to find out if it's a known type, and then parses the contents accordingly.
    /// </summary>
    /// <param name="btAdv">Bluetooth advertisement to parse.</param>
    public Beacon(BLEAdvertisementPacket btAdv)
    {
        BluetoothAddress = btAdv.BluetoothAddress;
        UpdateBeacon(btAdv);
    }

    /// <summary>
    /// Manually create a new Beacon instance.
    /// </summary>
    /// <param name="beaconType">Beacon type to use for this manually constructed beacon.</param>
    public Beacon(BeaconTypeEnum beaconType)
    {
        BeaconType = beaconType;
    }

    /// <summary>
    /// Received a new advertisement for this beacon.
    /// If the Bluetooth address of the new advertisement matches this beacon,
    /// it will parse the contents of the advertisement and extract known frames.
    /// </summary>
    /// <param name="btAdv">Bluetooth advertisement to parse.</param>
    /// <exception cref="BeaconException">The Bluetooth address of the advertisement does not match this beacon.</exception>
    public void UpdateBeacon(BLEAdvertisementPacket? btAdv)
    {
        if (btAdv == null) return;

        // Only update beacon info if it's the same beacon
        if (btAdv.BluetoothAddress != BluetoothAddress)
        {
            throw new BeaconException("Bluetooth address of beacon does not match - not updating beacon information");
        }

        Rssi = btAdv.RawSignalStrengthInDBm;
        Timestamp = btAdv.Timestamp;

        // Check if beacon advertisement contains any actual usable data
        var advertisement = btAdv.Advertisement;
        if (advertisement == null) return;

        // Eddystone beacons are identified via the service UUID and / or the service data;
        // iBeacons are identified via the manufacturer data.
        if (BeaconType == BeaconTypeEnum.Unknown && IsEddystoneAdvertisement(advertisement))
        {
            BeaconType = BeaconTypeEnum.Eddystone;
        }

        if (BeaconType == BeaconTypeEnum.Eddystone)
        {
            ParseEddystoneData(advertisement);
        }

        ParseManufacturerData(advertisement);
    }

    private static bool IsEddystoneAdvertisement(BLEAdvertisement advertisement)
    {
        foreach (var serviceUuid in advertisement.ServiceUuids)
        {
            if (serviceUuid == EddystoneGuid) return true;
        }
        foreach (var dataSection in advertisement.DataSections)
        {
            if (IsEddystoneServiceData(dataSection)) return true;
        }
        return false;
    }

    private static bool IsEddystoneServiceData(BLEAdvertisementDataSection dataSection) =>
        (dataSection.DataType == ServiceData16BitUuidType || dataSection.Manufacturer == (ushort)BTMember.GoogleEddystone) &&
        dataSection.Data.IsEddystoneFrameType();

    private void ParseManufacturerData(BLEAdvertisement advertisement)
    {
        // Manufacturer data - used by Proximity Beacon type (iBeacon)
        foreach (var manufacturerData in advertisement.ManufacturerData)
        {
            if (!BeaconFrameHelper.IsProximityBeaconPayload(manufacturerData.CompanyId, manufacturerData.Data)) continue;

            BeaconType = BeaconTypeEnum.iBeacon;
            var beaconFrame = new ProximityBeaconFrame(manufacturerData.Data);

            // Only one relevant data frame for iBeacons
            var existingFrame = FindFrame(f => f is ProximityBeaconFrame);
            if (existingFrame != null)
            {
                existingFrame.Update(beaconFrame);
            }
            else
            {
                BeaconFrames.Add(beaconFrame);
            }
        }
    }

    private void ParseEddystoneData(BLEAdvertisement advertisement)
    {
        // Relevant data of Eddystone is in service data sections (AD type 0x16):
        // 0x16 = 0xAA 0xFE [frame type] [data]
        foreach (var dataSection in advertisement.DataSections)
        {
            if (!IsEddystoneServiceData(dataSection)) continue;

            var beaconFrame = dataSection.Data.CreateEddystoneBeaconFrame();
            if (beaconFrame == null) continue;

            var existingFrame = FindFrame(f => IsSameFrameKind(f, beaconFrame));
            if (existingFrame != null)
            {
                // If we know this frame already, update the existing info.
                existingFrame.Update(beaconFrame);
            }
            else
            {
                // We didn't know this beacon frame so far - add it to the list of known frames.
                BeaconFrames.Add(beaconFrame);
            }
        }
    }

    private static bool IsSameFrameKind(BeaconFrameBase existing, BeaconFrameBase newFrame)
    {
        if (existing.GetType() != newFrame.GetType()) return false;
        if (newFrame is not UnknownBeaconFrame) return true;

        // Unknown frames - also compare the Eddystone frame type
        var existingFrameType = existing.Payload.GetEddystoneFrameType();
        return existingFrameType != null && existingFrameType == newFrame.Payload.GetEddystoneFrameType();
    }

    private BeaconFrameBase? FindFrame(Func<BeaconFrameBase, bool> predicate)
    {
        foreach (var frame in BeaconFrames)
        {
            if (predicate(frame)) return frame;
        }
        return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
