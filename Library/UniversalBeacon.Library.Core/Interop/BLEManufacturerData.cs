using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{
    // https://www.bluetooth.com/specifications/assigned-numbers/16-bit-uuids-for-members
    public enum BTMember
    {
        Estimote = 0xFE9A,
        GoogleEddystone = 0xFEAA
    }

    // https://www.bluetooth.com/specifications/assigned-numbers/company-identifiers
    public enum BTCompany
    {
    }

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

    public class FlagData : BLERecord
    {
        public byte Flags { get; private set; }

        public FlagData(BLEPacketType packetType, byte flags)
            : base(packetType)
        {
            Flags = flags;
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


    public class GenericRecord : BLERecord
    {
        public byte[] RawData { get; private set; }

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
