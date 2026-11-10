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
using System.Globalization;

namespace UniversalBeacon.Library;

internal static class AndroidPacketExtensions
{
    /// <summary>
    /// Convert a Bluetooth address string ("C4:7F:51:0A:12:34") to its numeric representation.
    /// </summary>
    public static ulong ToNumericAddress(this string addressString)
    {
        ulong address = 0;
        foreach (var part in addressString.Split([':'], StringSplitOptions.RemoveEmptyEntries))
        {
            address = (address << 8) | byte.Parse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
        return address;
    }
}
