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
/// Platform-independent contents of a Bluetooth LE advertisement.
/// </summary>
public sealed class BLEAdvertisement
{
    public string? LocalName { get; set; }

    /// <summary>
    /// Advertised service UUIDs (16-, 32- and 128-bit UUIDs, expanded to 128 bit).
    /// </summary>
    public List<Guid> ServiceUuids { get; set; } = [];

    /// <summary>
    /// Service data sections.
    /// </summary>
    public List<BLEAdvertisementDataSection> DataSections { get; set; } = [];

    public List<BLEManufacturerData> ManufacturerData { get; set; } = [];

    /// <summary>
    /// Parse a raw advertisement / scan response payload, consisting of
    /// [length] [AD type] [data] structures, into a new advertisement.
    /// Malformed structures are skipped.
    /// </summary>
    /// <param name="rawData">Raw advertising data, for example from ScanRecord.GetBytes() on Android.</param>
    /// <returns>The parsed advertisement.</returns>
    public static BLEAdvertisement Parse(byte[]? rawData)
    {
        var result = new BLEAdvertisement();
        foreach (var record in BLERecordParser.Parse(rawData))
        {
            switch (record)
            {
                case BLEManufacturerData manufacturerData:
                    result.ManufacturerData.Add(manufacturerData);
                    break;
                case BLEAdvertisementDataSection dataSection:
                    result.DataSections.Add(dataSection);
                    break;
                case BLENameRecord nameRecord:
                    // Prefer the complete name over a shortened name.
                    if (result.LocalName == null || nameRecord.PacketType == BLEPacketType.LocalName)
                    {
                        result.LocalName = nameRecord.Name;
                    }
                    break;
                case BLEUuidListRecord uuidList:
                    foreach (var uuid in uuidList.Uuids)
                    {
                        if (!result.ServiceUuids.Contains(uuid)) result.ServiceUuids.Add(uuid);
                    }
                    break;
            }
        }
        return result;
    }
}
