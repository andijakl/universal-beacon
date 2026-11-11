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
using System.Runtime.CompilerServices;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// Service data (AD types 0x16, 0x20, 0x21) that is not parsed as a known beacon format,
/// e.g., Google Fast Pair (0xFE2C). The payload is the data after the service UUID.
/// One frame is kept per service UUID and updated with the latest data.
/// </summary>
public class ServiceDataFrame : BeaconFrameBase
{
    /// <param name="serviceUuid">Full 128-bit service UUID.</param>
    /// <param name="data">Service data without the service UUID.</param>
    public ServiceDataFrame(Guid serviceUuid, byte[] data) : base(data)
    {
        ServiceUuid = serviceUuid;
    }

    /// <summary>
    /// Full 128-bit service UUID.
    /// </summary>
    public Guid ServiceUuid { get; }

    /// <summary>
    /// Service UUID in its shortest form, e.g., "0xFE2C" for 16-bit UUIDs.
    /// </summary>
    public string ServiceUuidAsString => BluetoothUuids.ToShortString(ServiceUuid);

    /// <summary>
    /// Data formatted as upper-case hex string.
    /// </summary>
    public string DataAsString => Payload == null ? string.Empty : ByteHelpers.ToHex(Payload, 0, Payload.Length);

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(Payload)) base.OnPropertyChanged(nameof(DataAsString));
    }
}
