// Copyright 2015 - 2026 Andreas Jakl. All rights reserved.
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
using System.Text;

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// Endianness-independent helpers for reading and writing beacon payloads.
/// Beacon specifications (Eddystone, iBeacon) store multi-byte values in big-endian order.
/// </summary>
internal static class ByteHelpers
{
    /// <summary>
    /// Bluetooth base UUID, used to expand 16- and 32-bit UUIDs: 0000xxxx-0000-1000-8000-00805F9B34FB.
    /// </summary>
    private const string BluetoothBaseUuidSuffix = "-0000-1000-8000-00805F9B34FB";

    public static ushort ReadUInt16BigEndian(byte[] data, int offset) =>
        (ushort)((data[offset] << 8) | data[offset + 1]);

    public static uint ReadUInt32BigEndian(byte[] data, int offset) =>
        ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];

    public static void WriteUInt16BigEndian(byte[] data, int offset, ushort value)
    {
        data[offset] = (byte)(value >> 8);
        data[offset + 1] = (byte)value;
    }

    public static void WriteUInt32BigEndian(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }

    public static byte[] Slice(byte[] data, int offset, int length)
    {
        var result = new byte[length];
        Buffer.BlockCopy(data, offset, result, 0, length);
        return result;
    }

    public static byte[]? CopyOrNull(byte[]? data)
    {
        if (data == null) return null;
        var result = new byte[data.Length];
        Buffer.BlockCopy(data, 0, result, 0, data.Length);
        return result;
    }

    public static bool BytesEqual(byte[]? a, byte[]? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null || a.Length != b.Length) return false;
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    public static string ToHex(byte[] data, int offset, int length)
    {
        var sb = new StringBuilder(length * 2);
        for (var i = offset; i < offset + length; i++)
        {
            sb.Append(data[i].ToString("X2"));
        }
        return sb.ToString();
    }

    public static byte[] FromHex(string hex)
    {
        var result = new byte[hex.Length / 2];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return result;
    }

    /// <summary>
    /// Creates a Guid from 16 bytes in big-endian (network / display) order,
    /// as used by beacon payloads. Note that <see cref="Guid(byte[])"/> uses a mixed-endian layout instead.
    /// </summary>
    public static Guid GuidFromBigEndian(byte[] data, int offset) =>
        Guid.ParseExact(ToHex(data, offset, 16), "N");

    /// <summary>
    /// Returns the 16 bytes of the Guid in big-endian (network / display) order.
    /// </summary>
    public static byte[] GuidToBigEndian(Guid guid) => FromHex(guid.ToString("N"));

    /// <summary>
    /// Creates a full 128-bit Guid from a Bluetooth UUID in big-endian byte order.
    /// Supports 16-bit, 32-bit and 128-bit UUIDs.
    /// </summary>
    public static Guid? GuidFromBluetoothUuid(byte[] bigEndianUuid)
    {
        switch (bigEndianUuid.Length)
        {
            case 2:
                return Guid.Parse("0000" + ToHex(bigEndianUuid, 0, 2) + BluetoothBaseUuidSuffix);
            case 4:
                return Guid.Parse(ToHex(bigEndianUuid, 0, 4) + BluetoothBaseUuidSuffix);
            case 16:
                return GuidFromBigEndian(bigEndianUuid, 0);
            default:
                return null;
        }
    }
}
