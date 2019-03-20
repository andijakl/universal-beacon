// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;

namespace UniversalBeacon.Library.Core.Interop
{
    public sealed class BLEManufacturerData : BLERecord
    {
        public ushort CompanyId { get; set; }
        public byte[] Data { get; set; }

        public BLEManufacturerData(BLEPacketType packetType, byte[] data)
            : base(packetType)
        {
            CompanyId = BitConverter.ToUInt16(data, 0);
            Data = new byte[data.Length - 2];
            Buffer.BlockCopy(data, 2, Data, 0, Data.Length);
        }

        public BLEManufacturerData()
            : base(BLEPacketType.ManufacturerData)
        {
        }

    }
}
