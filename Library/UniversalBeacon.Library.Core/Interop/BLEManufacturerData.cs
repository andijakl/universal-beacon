using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{
    public enum BLEPacketType
    {
        Invalid = 0x00,
        Flags = 0x01,
        LocalName = 0x09,
        UUID16List = 0x03,
        UUID16 = 0x16,
        ManufacturerData = 0xff
    }

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
            : base( BLEPacketType.ManufacturerData)
        {
        }

    }

    public class NameData : BLERecord
    {
        private string m_name;

        public NameData(BLEPacketType packetType, byte[] data)
            : base(packetType)
        {
            m_name = Encoding.ASCII.GetString(data);
        }

        public string Name
        {
            get { return m_name; }
        }
    }

    public class GenericRecord : BLERecord
    {
        public byte[] RawData { get; set; }

        public GenericRecord(BLEPacketType packetType, byte[] data)
            : base(packetType)
        {
            RawData = data;
        }
    }

    public abstract class BLERecord
    {
        public BLEPacketType PacketType { get; set; }

        public BLERecord(BLEPacketType packetType)
        {
            PacketType = packetType;
        }
    }
}
