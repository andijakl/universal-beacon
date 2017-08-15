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
            Data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, Data, 0, Data.Length);

            // TODO:        
            // it's unclear to me where this comes from
            // The WIndows API sets it for Eddystone packets, but I can't find it in any documentation
            //DataType = 0x16;
        }
    }

}
