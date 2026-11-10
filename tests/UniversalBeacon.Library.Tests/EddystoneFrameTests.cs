using System.Numerics;
using UniversalBeacon.Library.Core.Entities;
using static UniversalBeacon.Library.Tests.TestData;

namespace UniversalBeacon.Library.Tests;

public class EddystoneFrameTests
{
    [Fact]
    public void Uid_ParsesAllFields()
    {
        var frame = Assert.IsType<UidEddystoneFrame>(Hex(UidPayload).CreateEddystoneBeaconFrame());

        Assert.Equal(-18, frame.RangingData);
        Assert.Equal(Hex("EDD1EBEAC04E5DEFA017"), frame.NamespaceId);
        Assert.Equal("EDD1EBEAC04E5DEFA017", frame.NamespaceIdAsString);
        Assert.Equal(Hex("0BDB87539B67"), frame.InstanceId);
        Assert.Equal(0x0BDB87539B67UL, frame.InstanceIdAsNumber);
    }

    [Fact]
    public void Uid_NamespaceIdAsNumber_IsUnsigned()
    {
        var frame = new UidEddystoneFrame(Hex(UidPayload));

        // The first namespace byte is 0xED - must not be interpreted as negative number.
        Assert.Equal(BigInteger.Parse("0EDD1EBEAC04E5DEFA017", System.Globalization.NumberStyles.HexNumber),
            frame.NamespaceIdAsNumber);
        Assert.True(frame.NamespaceIdAsNumber > 0);
    }

    [Fact]
    public void Uid_WithoutReservedBytes_IsAccepted()
    {
        var payload = Hex(UidPayload)[..^2];

        var frame = Assert.IsType<UidEddystoneFrame>(payload.CreateEddystoneBeaconFrame());

        Assert.Equal("0BDB87539B67", frame.InstanceIdAsString);
    }

    [Fact]
    public void Uid_CreatedFromValues_EncodesPayload()
    {
        var frame = new UidEddystoneFrame(-18, Hex("EDD1EBEAC04E5DEFA017"), Hex("0BDB87539B67"));

        Assert.Equal(Hex(UidPayload), frame.Payload);
        Assert.True(frame.IsValid());
    }

    [Fact]
    public void Uid_WrongLength_BecomesUnknownFrame()
    {
        var frame = Hex("AA FE 00 EE 0102").CreateEddystoneBeaconFrame();

        Assert.IsType<UnknownBeaconFrame>(frame);
    }

    [Theory]
    [InlineData(UrlPayload, "https://www.google.com/")]
    [InlineData("AA FE 10 00 02 6578616D706C65 08", "http://example.org")]
    [InlineData("AA FE 10 00 03 676F6F2E676C 2F616263", "https://goo.gl/abc")]
    public void Url_DecodesUrl(string payload, string expectedUrl)
    {
        var frame = Assert.IsType<UrlEddystoneFrame>(Hex(payload).CreateEddystoneBeaconFrame());

        Assert.Equal(expectedUrl, frame.CompleteUrl);
    }

    [Fact]
    public void Url_CreatedFromValues_EncodesPayload()
    {
        var frame = new UrlEddystoneFrame(-21, "https://www.google.com/");

        Assert.Equal(-21, frame.RangingData);
        Assert.Equal(Hex(UrlPayload), frame.Payload);
    }

    [Fact]
    public void Url_InvalidScheme_HasNoPayload()
    {
        var frame = new UrlEddystoneFrame(0, "ftp://example.com");

        Assert.Null(frame.Payload);
    }

    [Fact]
    public void Url_ReservedSchemeByte_BecomesUnknownFrame()
    {
        Assert.IsType<UnknownBeaconFrame>(Hex("AA FE 10 00 09 41").CreateEddystoneBeaconFrame());
    }

    [Fact]
    public void Tlm_ParsesAllFields()
    {
        var frame = Assert.IsType<TlmEddystoneFrame>(Hex(TlmPayload).CreateEddystoneBeaconFrame());

        Assert.Equal(0, frame.Version);
        Assert.Equal(3000, frame.BatteryInMilliV);
        Assert.Equal(24.5f, frame.TemperatureInC);
        Assert.Equal(256u, frame.AdvertisementFrameCount);
        Assert.Equal(10u, frame.TimeSincePowerUp);
        Assert.Equal(TimeSpan.FromSeconds(1), frame.TimeSincePowerUpAsTimeSpan);
    }

    [Theory]
    [InlineData("F5C0", -10.25f)]
    [InlineData("8000", -128f)]
    [InlineData("0000", 0f)]
    public void Tlm_ParsesSignedFixedPointTemperature(string temperatureHex, float expected)
    {
        var payload = Hex($"AA FE 20 00 0BB8 {temperatureHex} 00000100 0000000A");

        var frame = new TlmEddystoneFrame(payload);

        Assert.Equal(expected, frame.TemperatureInC);
    }

    [Fact]
    public void Tlm_CreatedFromValues_EncodesPayload()
    {
        var frame = new TlmEddystoneFrame(0, 3000, 24.5f, 256, 10);

        Assert.Equal(Hex(TlmPayload), frame.Payload);

        frame.TemperatureInC = -10.25f;
        Assert.Equal(Hex("F5C0"), frame.Payload![6..8]);
    }

    [Fact]
    public void Tlm_Truncated_BecomesUnknownFrameWithoutException()
    {
        var frame = Hex("AA FE 20 00 0BB8").CreateEddystoneBeaconFrame();

        Assert.IsType<UnknownBeaconFrame>(frame);
    }

    [Fact]
    public void Tlm_EncryptedPayload_IsNotParsedAsPlainTelemetry()
    {
        var frame = new TlmEddystoneFrame(Hex(EncryptedTlmPayload));

        Assert.False(frame.IsValid());
        Assert.Equal(0, frame.BatteryInMilliV);
    }

    [Fact]
    public void EncryptedTlm_ParsesAllFields()
    {
        var frame = Assert.IsType<EncryptedTlmEddystoneFrame>(Hex(EncryptedTlmPayload).CreateEddystoneBeaconFrame());

        Assert.Equal(Hex("0102030405060708090A0B0C"), frame.EncryptedData);
        Assert.Equal("0102030405060708090A0B0C", frame.EncryptedDataAsString);
        Assert.Equal(0x1234, frame.Salt);
        Assert.Equal(0xABCD, frame.MessageIntegrityCheck);
    }

    [Fact]
    public void Eid_ParsesAllFields()
    {
        var frame = Assert.IsType<EidEddystoneFrame>(Hex(EidPayload).CreateEddystoneBeaconFrame());

        Assert.Equal(-16, frame.RangingData);
        Assert.Equal("1122334455667788", frame.EphemeralIdentifierAsString);
    }

    [Fact]
    public void Eid_CreatedFromValues_EncodesPayload()
    {
        var frame = new EidEddystoneFrame(-16, Hex("1122334455667788"));

        Assert.Equal(Hex(EidPayload), frame.Payload);
    }

    [Fact]
    public void Eid_Truncated_BecomesUnknownFrame()
    {
        Assert.IsType<UnknownBeaconFrame>(Hex("AA FE 30 F0 1122").CreateEddystoneBeaconFrame());
    }

    [Fact]
    public void ReservedFrameType_BecomesUnknownFrame()
    {
        var frame = Hex("AA FE 50 01 02 03").CreateEddystoneBeaconFrame();

        var unknown = Assert.IsType<UnknownBeaconFrame>(frame);
        Assert.Equal((BeaconFrameHelper.EddystoneFrameType)0x50, unknown.Payload.GetEddystoneFrameType());
    }

    [Theory]
    [InlineData("FE AA 00 01")]
    [InlineData("AA FE")]
    [InlineData("")]
    public void NonEddystonePayload_ReturnsNull(string payload)
    {
        Assert.Null(Hex(payload).CreateEddystoneBeaconFrame());
    }

    [Fact]
    public void Update_RaisesPropertyChangedOnlyForChangedValues()
    {
        var frame = new TlmEddystoneFrame(Hex(TlmPayload));
        var changed = new List<string?>();
        frame.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        frame.Update(new TlmEddystoneFrame(Hex("AA FE 20 00 0BB8 1880 00000101 0000000A")));

        Assert.Contains(nameof(TlmEddystoneFrame.AdvertisementFrameCount), changed);
        Assert.DoesNotContain(nameof(TlmEddystoneFrame.BatteryInMilliV), changed);
        Assert.Equal(257u, frame.AdvertisementFrameCount);
    }
}
