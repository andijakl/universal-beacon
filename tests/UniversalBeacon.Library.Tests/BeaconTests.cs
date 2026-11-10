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
    public void OtherAdvertisement_RemainsUnknown()
    {
        var raw = AdStructure(0xFF, Hex("5900 0102030405"));

        var beacon = new Beacon(Packet(Address, raw));

        Assert.Equal(Beacon.BeaconTypeEnum.Unknown, beacon.BeaconType);
        Assert.Empty(beacon.BeaconFrames);
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
