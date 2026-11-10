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
/// Manufacturer specific data record (AD type 0xFF).
/// </summary>
public sealed class BLEManufacturerData : BLERecord
{
    /// <summary>
    /// Bluetooth company identifier, e.g., 0x004C for Apple.
    /// </summary>
    public ushort CompanyId { get; set; }

    /// <summary>
    /// Manufacturer data without the company identifier.
    /// </summary>
    public byte[] Data { get; set; } = [];

    /// <summary>
    /// Create the record from the raw record data, which starts with the
    /// company identifier (2 bytes, little-endian), followed by the data.
    /// </summary>
    public BLEManufacturerData(BLEPacketType packetType, byte[] data)
        : base(packetType)
    {
        if (data.Length < 2)
        {
            Data = data;
            return;
        }
        CompanyId = (ushort)(data[0] | (data[1] << 8));
        Data = new byte[data.Length - 2];
        Buffer.BlockCopy(data, 2, Data, 0, Data.Length);
    }

    public BLEManufacturerData()
        : base(BLEPacketType.ManufacturerData)
    {
    }
}
