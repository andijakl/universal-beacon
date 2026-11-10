// Copyright 2026 Andreas Jakl. All rights reserved.
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
using System.Collections.Generic;
using UniversalBeacon.Library.Core.Entities;

namespace UniversalBeacon.Library.Core.Interop;

/// <summary>
/// List of 16-, 32- or 128-bit service UUIDs, expanded to full 128-bit UUIDs.
/// </summary>
public sealed class BLEUuidListRecord : BLERecord
{
    public IReadOnlyList<Guid> Uuids { get; }

    /// <summary>
    /// Parse the UUID list from the raw record data (little-endian UUIDs).
    /// </summary>
    /// <param name="packetType">One of the UUID list AD types.</param>
    /// <param name="data">Raw record data.</param>
    /// <param name="uuidLength">Length of a single UUID in bytes: 2, 4 or 16.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="uuidLength"/> is not 2, 4 or 16.</exception>
    public BLEUuidListRecord(BLEPacketType packetType, byte[] data, int uuidLength)
        : base(packetType)
    {
        if (uuidLength != 2 && uuidLength != 4 && uuidLength != 16)
        {
            throw new ArgumentOutOfRangeException(nameof(uuidLength), uuidLength,
                "Bluetooth UUIDs are 2, 4 or 16 bytes long.");
        }

        var uuids = new List<Guid>();
        for (var offset = 0; offset + uuidLength <= data.Length; offset += uuidLength)
        {
            // Convert from little-endian (on air) to big-endian.
            var bigEndian = new byte[uuidLength];
            for (var i = 0; i < uuidLength; i++)
            {
                bigEndian[i] = data[offset + uuidLength - 1 - i];
            }
            var uuid = ByteHelpers.GuidFromBluetoothUuid(bigEndian);
            if (uuid != null) uuids.Add(uuid.Value);
        }
        Uuids = uuids;
    }
}
