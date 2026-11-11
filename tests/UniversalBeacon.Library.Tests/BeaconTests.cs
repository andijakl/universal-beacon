using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interop;
using static UniversalBeacon.Library.Tests.TestData;

namespace UniversalBeacon.Library.Tests;

public class BeaconTests
{
    private const ulong Address = 0xC47F510A1234;

    [Fact]
    public void Eddystone_FramesAreCollectedAndUpdated()
    {
        var beacon = new Beacon(Packet(Address, RawEddystoneAdvertisement(Hex(UidPayload))));
        beacon.UpdateBeacon(Packet(Address, RawEddystoneAdvertisement(Hex(TlmPayload))));
        beacon.UpdateBeacon(Packet(Address, RawEddystoneAdvertisement(Hex(EncryptedTlmPayload))));
        beacon.UpdateBeacon(Packet(Address, RawEddystoneAdvertisement(Hex("AA FE 20 00 0BB8 1880 00000200 0000000A")), rssi: -42));

        Assert.Equal(Beacon.BeaconTypeEnum.Eddystone, beacon.BeaconType);
        Assert.Equal(3, beacon.BeaconFrames.Count);
        Assert.IsType<UidEddystoneFrame>(beacon.BeaconFrames[0]);
        var tlm = Assert.IsType<TlmEddystoneFrame>(beacon.BeaconFrames[1]);
        Assert.Equal(512u, tlm.AdvertisementFrameCount);
        Assert.IsType<EncryptedTlmEddystoneFrame>(beacon.BeaconFrames[2]);
        Assert.Equal(-42, beacon.Rssi);
    }

    [Fact]
    public void UnknownEddystoneFrames_AreKeptPerFrameType()
    {
        var beacon = new Beacon(Packet(Address, RawEddystoneAdvertisement(Hex("AA FE 50 01"))));
        beacon.UpdateBeacon(Packet(Address, RawEddystoneAdvertisement(Hex("AA FE 50 02"))));
        beacon.UpdateBeacon(Packet(Address, RawEddystoneAdvertisement(Hex("AA FE 60 01"))));

        Assert.Equal(2, beacon.BeaconFrames.Count);
        Assert.Equal(Hex("AA FE 50 02"), beacon.BeaconFrames[0].Payload);
    }

    [Fact]
    public void FindHubAccessory_WithoutServiceUuidList_IsDetected()
    {
        var serviceData = Concat(Hex("AA FE 40"), Repeat(0x22, 20));

        var beacon = new Beacon(Packet(Address, RawEddystoneAdvertisement(serviceData, includeUuidList: false)));

        Assert.Equal(Beacon.BeaconTypeEnum.Eddystone, beacon.BeaconType);
        Assert.IsType<FindHubNetworkFrame>(Assert.Single(beacon.BeaconFrames));
    }

    [Fact]
    public void IBeacon_IsDetectedAndUpdated()
    {
        byte[] Raw(string minor) => AdStructure(0xFF,
            Concat(Hex("4C00"), Hex($"02 15 E2C56DB5DFFB48D2B060D0F5A71096E0 0001 {minor} C5")));

        var beacon = new Beacon(Packet(Address, Raw("0002")));
        beacon.UpdateBeacon(Packet(Address, Raw("0003")));

        Assert.Equal(Beacon.BeaconTypeEnum.iBeacon, beacon.BeaconType);
        var frame = Assert.IsType<ProximityBeaconFrame>(Assert.Single(beacon.BeaconFrames));
        Assert.Equal(3, frame.Minor);
    }

    [Fact]
    public void OtherAdvertisement_RemainsUnknownButKeepsRawData()
    {
        // The test beacon from the question: service UUID 0x4664, company ID 0x9999, data A1B2
        var raw = Concat(
            AdStructure(0x01, [0x06]),
            AdStructure(0x03, Hex("6446")),
            AdStructure(0xFF, Hex("9999 A1B2")),
            AdStructure(0x09, "Test"u8.ToArray()));

        var beacon = new Beacon(Packet(Address, raw));

        Assert.Equal(Beacon.BeaconTypeEnum.Unknown, beacon.BeaconType);
        Assert.Equal("Test", beacon.LocalName);
        Assert.Equal("0x4664", BluetoothUuids.ToShortString(Assert.Single(beacon.ServiceUuids)));
        var frame = Assert.IsType<ManufacturerDataFrame>(Assert.Single(beacon.BeaconFrames));
        Assert.Equal(0x9999, frame.CompanyId);
        Assert.Equal("0x9999", frame.CompanyIdAsString);
        Assert.Equal("A1B2", frame.DataAsString);
    }

    [Fact]
    public void ManufacturerData_IsKeptPerCompanyAndUpdated()
    {
        var beacon = new Beacon(Packet(Address, AdStructure(0xFF, Hex("0600 01"))));
        beacon.UpdateBeacon(Packet(Address, AdStructure(0xFF, Hex("4C00 1005"))));
        var changed = new List<string?>();
        beacon.BeaconFrames[0].PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        beacon.UpdateBeacon(Packet(Address, AdStructure(0xFF, Hex("0600 02"))));

        Assert.Equal(2, beacon.BeaconFrames.Count);
        Assert.Equal("02", ((ManufacturerDataFrame)beacon.BeaconFrames[0]).DataAsString);
        Assert.Contains(nameof(ManufacturerDataFrame.DataAsString), changed);
    }

    [Fact]
    public void ServiceData_OtherThanEddystone_IsKeptAsRawFrame()
    {
        var raw = Concat(
            AdStructure(0x16, Hex("2CFE 00B1C2")),                        // Google Fast Pair
            AdStructure(0x21, Concat(Hex("9ECADC240EE5A9E093F3A3B50100406E"), Hex("FF"))));

        var beacon = new Beacon(Packet(Address, raw));

        Assert.Equal(Beacon.BeaconTypeEnum.Unknown, beacon.BeaconType);
        Assert.Equal(2, beacon.BeaconFrames.Count);
        var fastPair = Assert.IsType<ServiceDataFrame>(beacon.BeaconFrames[0]);
        Assert.Equal("0xFE2C", fastPair.ServiceUuidAsString);
        Assert.Equal("00B1C2", fastPair.DataAsString);
        var custom = Assert.IsType<ServiceDataFrame>(beacon.BeaconFrames[1]);
        Assert.Equal("6E400001-B5A3-F393-E0A9-E50E24DCCA9E", custom.ServiceUuidAsString);
    }

    [Fact]
    public void EddystoneServiceData_IsNotDuplicatedAsRawFrame()
    {
        var beacon = new Beacon(Packet(Address, RawEddystoneAdvertisement(Hex(UidPayload))));

        Assert.IsType<UidEddystoneFrame>(Assert.Single(beacon.BeaconFrames));
    }

    [Fact]
    public void MixedBeacon_KeepsEddystoneFramesAfterIBeaconDetection()
    {
        var iBeacon = AdStructure(0xFF, Concat(Hex("4C00"), Hex(IBeaconManufacturerData)));

        var beacon = new Beacon(Packet(Address, iBeacon));
        beacon.UpdateBeacon(Packet(Address, RawEddystoneAdvertisement(Hex(TlmPayload))));

        Assert.Equal(Beacon.BeaconTypeEnum.iBeacon, beacon.BeaconType);
        Assert.Contains(beacon.BeaconFrames, f => f is TlmEddystoneFrame);
    }

    [Fact]
    public void BeaconType_RaisesPropertyChanged()
    {
        var beacon = new Beacon(Packet(Address, []));
        var changed = new List<string?>();
        beacon.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        beacon.UpdateBeacon(Packet(Address, RawEddystoneAdvertisement(Hex(UidPayload))));

        Assert.Contains(nameof(Beacon.BeaconType), changed);
    }

    [Fact]
    public void ServiceUuids_AreAccumulatedWithoutDuplicates()
    {
        var beacon = new Beacon(Packet(Address, AdStructure(0x03, Hex("6446"))));
        beacon.UpdateBeacon(Packet(Address, AdStructure(0x03, Hex("6446 0F18"))));

        Assert.Equal(2, beacon.ServiceUuids.Count);
    }

    [Theory]
    [InlineData("00004664-0000-1000-8000-00805F9B34FB", "0x4664")]
    [InlineData("12345678-0000-1000-8000-00805F9B34FB", "0x12345678")]
    [InlineData("6e400001-b5a3-f393-e0a9-e50e24dcca9e", "6E400001-B5A3-F393-E0A9-E50E24DCCA9E")]
    public void BluetoothUuids_ToShortString(string uuid, string expected)
    {
        Assert.Equal(expected, BluetoothUuids.ToShortString(Guid.Parse(uuid)));
    }

    [Fact]
    public void BluetoothUuids_FromShortUuid()
    {
        Assert.Equal(EddystoneServiceUuid, BluetoothUuids.FromShortUuid(0xFEAA));
    }

    [Fact]
    public void ManuallyConstructedDataSection_WithoutDataType_IsStillParsed()
    {
        // Custom providers written for earlier versions did not set the DataType.
        var packet = new BLEAdvertisementPacket
        {
            BluetoothAddress = Address,
            Advertisement = new BLEAdvertisement
            {
                ServiceUuids = [EddystoneServiceUuid],
                DataSections = [new BLEAdvertisementDataSection { DataType = 0, Data = Hex(UidPayload) }]
            }
        };

        var beacon = new Beacon(packet);

        Assert.IsType<UidEddystoneFrame>(Assert.Single(beacon.BeaconFrames));
    }

    [Fact]
    public void UpdateBeacon_WithDifferentAddress_Throws()
    {
        var beacon = new Beacon(Packet(Address, RawEddystoneAdvertisement(Hex(UidPayload))));

        Assert.Throws<BeaconException>(() => beacon.UpdateBeacon(Packet(Address + 1, [])));
    }

    [Fact]
    public void BluetoothAddressAsString_IsFormatted()
    {
        var beacon = new Beacon(Beacon.BeaconTypeEnum.Unknown) { BluetoothAddress = Address };

        Assert.Equal("C4:7F:51:0A:12:34", beacon.BluetoothAddressAsString);
    }
}
