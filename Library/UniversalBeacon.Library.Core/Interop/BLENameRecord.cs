// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace UniversalBeacon.Library.Core.Interop
{
    public class BLENameRecord : BLERecord
    {
        public BLENameRecord(BLEPacketType packetType, byte[] data)
            : base(packetType)
        {
            Name = Encoding.ASCII.GetString(data);
        }

        public string Name { get; }
    }
}
