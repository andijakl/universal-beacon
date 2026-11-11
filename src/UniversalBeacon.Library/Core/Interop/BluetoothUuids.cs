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
using System.Globalization;

namespace UniversalBeacon.Library.Core.Interop;

/// <summary>
/// Helpers for Bluetooth UUIDs. 16- and 32-bit UUIDs are shortcuts for full 128-bit UUIDs
/// based on the Bluetooth base UUID 0000xxxx-0000-1000-8000-00805F9B34FB.
/// </summary>
public static class BluetoothUuids
{
    private const string BaseUuidSuffix = "-0000-1000-8000-00805f9b34fb";

    /// <summary>
    /// Expand a 16- or 32-bit Bluetooth UUID (e.g., 0xFEAA) to the full 128-bit UUID.
    /// </summary>
    public static Guid FromShortUuid(uint shortUuid) =>
        Guid.Parse(shortUuid.ToString("x8", CultureInfo.InvariantCulture) + BaseUuidSuffix);

    /// <summary>
    /// Get the 16- or 32-bit short form of a UUID based on the Bluetooth base UUID.
    /// </summary>
    /// <returns>True if the UUID is based on the Bluetooth base UUID.</returns>
    public static bool TryGetShortUuid(Guid uuid, out uint shortUuid)
    {
        var text = uuid.ToString("D");
        if (text.EndsWith(BaseUuidSuffix, StringComparison.OrdinalIgnoreCase))
        {
            shortUuid = uint.Parse(text.Substring(0, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return true;
        }
        shortUuid = 0;
        return false;
    }

    /// <summary>
    /// Format the UUID in its shortest form: "0x4664" for 16-bit UUIDs, "0x12345678" for
    /// 32-bit UUIDs, otherwise the full 128-bit UUID in upper case.
    /// </summary>
    public static string ToShortString(Guid uuid)
    {
        if (TryGetShortUuid(uuid, out var shortUuid))
        {
            return shortUuid <= 0xFFFF
                ? "0x" + shortUuid.ToString("X4", CultureInfo.InvariantCulture)
                : "0x" + shortUuid.ToString("X8", CultureInfo.InvariantCulture);
        }
        return uuid.ToString("D").ToUpperInvariant();
    }
}
