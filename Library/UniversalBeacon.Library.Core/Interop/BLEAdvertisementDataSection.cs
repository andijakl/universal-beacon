// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;

namespace UniversalBeacon.Library.Core.Interop
{
    public sealed class BLEAdvertisementDataSection : BLERecord
    {
        public ushort Manufacturer { get; private set; }

        public byte[] Data { get; set; }
        public byte DataType { get; set; }

        public BLEAdvertisementDataSection()
            : base(BLEPacketType.ServiceData)
        {
        }

        public BLEAdvertisementDataSection(BLEPacketType packetType, byte[] data)
            : base(packetType)
        {
            Manufacturer = BitConverter.ToUInt16(data, 0);
            Data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, Data, 0, Data.Length);

            // TODO:        
            // it's unclear to me where this comes from
            // The WIndows API sets it for Eddystone packets, but I can't find it in any documentation
            //DataType = 0x16;
        }
    }

}
