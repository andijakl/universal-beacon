using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalBeaconLibrary
{    
    public enum BLEAdvertisementType
    {
        ConnectableUndirected = 0,
        ConnectableDirected = 1,
        ScannableUndirected = 2,
        NonConnectableUndirected = 3,
        ScanResponse = 4
    }

    public sealed class BLEAdvertisement
    {
        public string LocalName { get; set; }
        public List<Guid> ServiceUuids { get; private set; }
        public List<BLEAdvertisementDataSection> DataSections { get; private set; }

        public BLEAdvertisement()
        {
            ServiceUuids = new List<Guid>();
            DataSections = new List<BLEAdvertisementDataSection>();
        }
/*
        public IReadOnlyList<BluetoothLEManufacturerData> GetManufacturerDataByCompanyId(ushort companyId);
        public IReadOnlyList<BluetoothLEAdvertisementDataSection> GetSectionsByType(byte type);

        public string LocalName { get; set; }
        public BluetoothLEAdvertisementFlags? Flags { get; set; }
        public IList<BluetoothLEAdvertisementDataSection> DataSections { get; }
        public IList<BluetoothLEManufacturerData> ManufacturerData { get; }
        public IList<Guid> ServiceUuids { get; }
        */
    }

    public sealed class BLEAdvertisementDataSection
    {
        public BLEAdvertisementDataSection()
        {
        }

        public BLEAdvertisementDataSection(byte dataType, IEnumerable<byte> data)
        {
        }

        public byte DataType { get; set; }
        public byte[] Data { get; set; }
    }

    public class BLEAdvertisementPacketArgs : EventArgs
    {
        public BLEAdvertisementPacket Data { get; private set; }

        public BLEAdvertisementPacketArgs(BLEAdvertisementPacket data)
        {
            Data = data;
        }
    }

    public class BLEAdvertisementPacket
    {
        public BLEAdvertisement Advertisement { get; set; }
        public BLEAdvertisementType AdvertisementType { get; set; }

        public ulong BluetoothAddress { get; set; }
        public short RawSignalStrengthInDBm { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public interface IBluetoothPacketProvider
    {
        event EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;
    }
}
