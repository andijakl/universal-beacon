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
/// Raw data section of an advertisement - most importantly service data (AD type 0x16).
/// For service data, <see cref="Data"/> starts with the service UUID in little-endian order
/// (e.g., 0xAA 0xFE for the Eddystone service UUID 0xFEAA), followed by the service payload.
/// </summary>
public sealed class BLEAdvertisementDataSection : BLERecord
{
    /// <summary>
    /// The first two bytes of the data, interpreted as little-endian 16-bit value.
    /// For 16-bit service data, this is the service UUID (e.g., 0xFEAA for Eddystone).
    /// </summary>
    public ushort Manufacturer => Data.Length >= 2 ? (ushort)(Data[0] | (Data[1] << 8)) : (ushort)0;

    public byte[] Data { get; set; } = [];

    /// <summary>
    /// AD type of this data section, e.g., 0x16 for service data with a 16-bit UUID.
    /// </summary>
    public byte DataType { get; set; } = (byte)BLEPacketType.ServiceData;

    public BLEAdvertisementDataSection()
        : base(BLEPacketType.ServiceData)
    {
    }

    public BLEAdvertisementDataSection(BLEPacketType packetType, byte[] data)
        : base(packetType)
    {
        DataType = (byte)packetType;
        Data = new byte[data.Length];
        Buffer.BlockCopy(data, 0, Data, 0, Data.Length);
    }
}
