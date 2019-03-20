// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

namespace UniversalBeacon.Library.Core.Interop
{
    // https://www.bluetooth.com/specifications/assigned-numbers/generic-access-profile
    public enum BLEPacketType
    {
        Invalid = 0x00,
        Flags = 0x01,
        LocalName = 0x09,
        UUID16List = 0x03,
        ServiceData = 0x16,
        ManufacturerData = 0xff
    }
}
