// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

namespace UniversalBeacon.Library.Core.Interop
{
    public class BLEFlagRecord : BLERecord
    {
        public byte Flags { get; private set; }

        public BLEFlagRecord(BLEPacketType packetType, byte flags)
            : base(packetType)
        {
            Flags = flags;
        }
    }
}
