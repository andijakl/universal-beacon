using UniversalBeacon.Library.Core.Interop;
using static UniversalBeacon.Library.Tests.TestData;

namespace UniversalBeacon.Library.Tests;

public class AdvertisementParserTests
{
    [Fact]
    public void Parse_EddystoneAdvertisement()
    {
        var advertisement = BLEAdvertisement.Parse(RawEddystoneAdvertisement(Hex(UidPayload)));

        Assert.Equal(new[] { EddystoneServiceUuid }, advertisement.ServiceUuids);
        var section = Assert.Single(advertisement.DataSections);
        Assert.Equal((byte)BLEPacketType.ServiceData, section.DataType);
        Assert.Equal((ushort)BTMember.GoogleEddystone, section.Manufacturer);
        Assert.Equal(Hex(UidPayload), section.Data);
        Assert.Empty(advertisement.ManufacturerData);
    }

    [Fact]
    public void Parse_IBeaconAdvertisement()
    {
        var raw = Concat(
            AdStructure(0x01, [0x06]),
            AdStructure(0xFF, Concat(Hex("4C00"), Hex(IBeaconManufacturerData))));

        var advertisement = BLEAdvertisement.Parse(raw);

        var manufacturerData = Assert.Single(advertisement.ManufacturerData);
        Assert.Equal(0x004C, manufacturerData.CompanyId);
        Assert.Equal(Hex(IBeaconManufacturerData), manufacturerData.Data);
    }

    [Fact]
    public void Parse_LocalNamePrefersCompleteName()
    {
        var raw = Concat(
            AdStructure(0x08, "Short"u8.ToArray()),
            AdStructure(0x09, "Complete Näme"u8.ToArray()));

        Assert.Equal("Complete Näme", BLEAdvertisement.Parse(raw).LocalName);
    }

    [Fact]
    public void Parse_ExpandsServiceUuids()
    {
        var raw = Concat(
            // 16-bit list: 0x180F (battery), 0xFEAA
            AdStructure(0x03, Hex("0F18 AAFE")),
            // 128-bit UUID 6E400001-B5A3-F393-E0A9-E50E24DCCA9E, little-endian on air
            AdStructure(0x07, Hex("9ECADC240EE5A9E093F3A3B50100406E")));

        var uuids = BLEAdvertisement.Parse(raw).ServiceUuids;

        Assert.Equal(new[]
        {
            Guid.Parse("0000180F-0000-1000-8000-00805F9B34FB"),
            EddystoneServiceUuid,
            Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E")
        }, uuids);
    }

    [Theory]
    [InlineData("1F 16 AAFE00")]          // length exceeds the data
    [InlineData("02 01")]                 // truncated
    [InlineData("01 FF")]                 // manufacturer data without company ID
    [InlineData("00 00 00")]              // padding only
    public void Parse_MalformedData_DoesNotThrow(string raw)
    {
        var advertisement = BLEAdvertisement.Parse(Hex(raw));

        Assert.Empty(advertisement.ManufacturerData);
        Assert.Empty(advertisement.DataSections);
    }

    [Fact]
    public void Parse_StopsAtPaddingButKeepsPreviousRecords()
    {
        var raw = Concat(AdStructure(0x03, Hex("AAFE")), Hex("00 00 00 00"));

        Assert.Single(BLEAdvertisement.Parse(raw).ServiceUuids);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(3)]
    public void UuidListRecord_InvalidUuidLength_Throws(int uuidLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new BLEUuidListRecord(BLEPacketType.UUID16List, Hex("AAFE"), uuidLength));
    }

    [Fact]
    public void Parse_Null_ReturnsEmptyAdvertisement()
    {
        var advertisement = BLEAdvertisement.Parse(null);

        Assert.Null(advertisement.LocalName);
        Assert.Empty(advertisement.ServiceUuids);
    }
}
