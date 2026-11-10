// Copyright 2015 - 2017 Chris Tacke. All rights reserved.
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

namespace UniversalBeacon.Library.Core.Interop;

/// <summary>
/// Platform-independent representation of a received Bluetooth LE advertisement.
/// </summary>
public class BLEAdvertisementPacket
{
    /// <summary>
    /// Parsed contents of the advertisement.
    /// </summary>
    public BLEAdvertisement? Advertisement { get; set; }

    public BLEAdvertisementType AdvertisementType { get; set; }

    /// <summary>
    /// Bluetooth address of the sender. On iOS and macOS, a stable value derived from the
    /// system-assigned peripheral identifier, as the MAC address is not available.
    /// </summary>
    public ulong BluetoothAddress { get; set; }

    /// <summary>
    /// Received signal strength in dBm.
    /// </summary>
    public short RawSignalStrengthInDBm { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}
