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

using System.Globalization;
using System.Runtime.CompilerServices;

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// Manufacturer specific data (AD type 0xFF) that is not parsed as a known beacon format.
/// The payload is the data after the 16-bit company identifier.
/// One frame is kept per company identifier and updated with the latest data.
/// </summary>
public class ManufacturerDataFrame : BeaconFrameBase
{
    /// <param name="companyId">Bluetooth company identifier, e.g., 0x004C for Apple.</param>
    /// <param name="data">Manufacturer data without the company identifier.</param>
    public ManufacturerDataFrame(ushort companyId, byte[] data) : base(data)
    {
        CompanyId = companyId;
    }

    /// <summary>
    /// Bluetooth company identifier, as assigned by the Bluetooth SIG.
    /// </summary>
    public ushort CompanyId { get; }

    /// <summary>
    /// Company identifier formatted as hex string, e.g., "0x004C".
    /// </summary>
    public string CompanyIdAsString => "0x" + CompanyId.ToString("X4", CultureInfo.InvariantCulture);

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
