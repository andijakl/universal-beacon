using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
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
            Data = new byte[data.Length - 2];
            Buffer.BlockCopy(data, 2, Data, 0, Data.Length);
        }
    }

}
