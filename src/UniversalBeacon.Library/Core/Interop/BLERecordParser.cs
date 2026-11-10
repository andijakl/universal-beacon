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
using System.Collections.Generic;

namespace UniversalBeacon.Library.Core.Interop;

/// <summary>
/// Parses raw Bluetooth LE advertising data into individual records.
/// </summary>
public static class BLERecordParser
{
    /// <summary>
    /// Parse the raw advertising data, consisting of [length] [AD type] [data] structures.
    /// Parsing stops at the first zero-length or truncated structure.
    /// </summary>
    /// <param name="recordData">Raw advertising data.</param>
    /// <returns>All parsed records.</returns>
    public static IReadOnlyList<BLERecord> Parse(byte[]? recordData)
    {
        var results = new List<BLERecord>();
        if (recordData == null) return results;

        var index = 0;
        while (index < recordData.Length)
        {
            var length = recordData[index++];
            // A length of 0 indicates the end of the significant part (padding follows).
            if (length == 0 || index + length > recordData.Length) break;

            var type = (BLEPacketType)recordData[index];
            var data = new byte[length - 1];
            Buffer.BlockCopy(recordData, index + 1, data, 0, data.Length);
            index += length;

            switch (type)
            {
                case BLEPacketType.Invalid:
                    break;
                case BLEPacketType.Flags:
                    if (data.Length > 0) results.Add(new BLEFlagRecord(type, data[0]));
                    break;
                case BLEPacketType.LocalName:
                case BLEPacketType.ShortenedLocalName:
                    results.Add(new BLENameRecord(type, data));
                    break;
                case BLEPacketType.ManufacturerData:
                    if (data.Length >= 2) results.Add(new BLEManufacturerData(type, data));
                    break;
                case BLEPacketType.ServiceData:
                case BLEPacketType.ServiceData32:
                case BLEPacketType.ServiceData128:
                    results.Add(new BLEAdvertisementDataSection(type, data));
                    break;
                case BLEPacketType.IncompleteUUID16List:
                case BLEPacketType.UUID16List:
                    results.Add(new BLEUuidListRecord(type, data, 2));
                    break;
                case BLEPacketType.IncompleteUUID32List:
                case BLEPacketType.CompleteUUID32List:
                    results.Add(new BLEUuidListRecord(type, data, 4));
                    break;
                case BLEPacketType.IncompleteUUID128List:
                case BLEPacketType.CompleteUUID128List:
                    results.Add(new BLEUuidListRecord(type, data, 16));
                    break;
                default:
                    results.Add(new BLEGenericRecord(type, data));
                    break;
            }
        }
        return results;
    }
}
