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

namespace UniversalBeacon.Library.Core.Interop;

/// <summary>
/// Advertising data (AD) types of Bluetooth LE advertisement records.
/// See the Bluetooth "Assigned Numbers" document, section "Common Data Types".
/// </summary>
public enum BLEPacketType
{
    Invalid = 0x00,
    Flags = 0x01,
    IncompleteUUID16List = 0x02,
    /// <summary>Complete list of 16-bit service UUIDs.</summary>
    UUID16List = 0x03,
    IncompleteUUID32List = 0x04,
    CompleteUUID32List = 0x05,
    IncompleteUUID128List = 0x06,
    CompleteUUID128List = 0x07,
    ShortenedLocalName = 0x08,
    LocalName = 0x09,
    TxPowerLevel = 0x0A,
    /// <summary>Service data with a 16-bit service UUID.</summary>
    ServiceData = 0x16,
    ServiceData32 = 0x20,
    ServiceData128 = 0x21,
    ManufacturerData = 0xFF
}
