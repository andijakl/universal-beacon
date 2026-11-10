using UniversalBeacon.Library.Core.Entities;
using static UniversalBeacon.Library.Tests.TestData;

namespace UniversalBeacon.Library.Tests;

public class FindHubAndProximityFrameTests
{
    [Fact]
    public void FindHub_ShortEidWithFlags_InUnwantedTrackingProtectionMode()
    {
        var payload = Concat(Hex("AA FE 41"), Repeat(0x5A, 20), [0x81]);

        var frame = Assert.IsType<FindHubNetworkFrame>(payload.CreateEddystoneBeaconFrame());

        Assert.True(frame.IsUnwantedTrackingProtectionModeActive);
        Assert.Equal(Repeat(0x5A, 20), frame.EphemeralIdentifier);
        Assert.Equal((byte)0x81, frame.HashedFlags);
    }

    [Fact]
    public void FindHub_LongEidWithoutFlags()
    {
        var payload = Concat(Hex("AA FE 40"), Repeat(0x11, 32));

        var frame = Assert.IsType<FindHubNetworkFrame>(payload.CreateEddystoneBeaconFrame());

        Assert.False(frame.IsUnwantedTrackingProtectionModeActive);
        Assert.Equal(32, frame.EphemeralIdentifier!.Length);
        Assert.Null(frame.HashedFlags);
    }

    [Fact]
    public void FindHub_InvalidLength_BecomesUnknownFrame()
    {
        var payload = Concat(Hex("AA FE 40"), Repeat(0x11, 25));

        Assert.IsType<UnknownBeaconFrame>(payload.CreateEddystoneBeaconFrame());
    }

    [Fact]
    public void Proximity_ParsesAllFields()
    {
        var data = Hex(IBeaconManufacturerData);

        Assert.True(BeaconFrameHelper.IsProximityBeaconPayload(ProximityBeaconFrame.CompanyId, data));
        var frame = new ProximityBeaconFrame(data);

        Assert.True(frame.IsValid());
        Assert.Equal(Guid.Parse(IBeaconUuid), frame.Uuid);
        Assert.Equal(IBeaconUuid.ToLowerInvariant(), frame.UuidAsString);
        Assert.Equal(1, frame.Major);
        Assert.Equal(2, frame.Minor);
        Assert.Equal("0001", frame.MajorAsString);
        Assert.Equal(-59, frame.TxPower);
    }

    [Fact]
    public void Proximity_CreatedFromValues_EncodesPayload()
    {
        var frame = new ProximityBeaconFrame(Guid.Parse(IBeaconUuid), 1, 2, -59);

        Assert.Equal(Hex(IBeaconManufacturerData), frame.Payload);
    }

    [Fact]
    public void Proximity_OtherCompanyOrTooShort_IsRejected()
    {
        var data = Hex(IBeaconManufacturerData);

        Assert.False(BeaconFrameHelper.IsProximityBeaconPayload(0x0059, data));
        Assert.False(BeaconFrameHelper.IsProximityBeaconPayload(ProximityBeaconFrame.CompanyId, data[..10]));
        Assert.False(BeaconFrameHelper.IsProximityBeaconPayload(ProximityBeaconFrame.CompanyId, null));
    }

    [Fact]
    public void Proximity_InvalidPayload_DoesNotThrow()
    {
        var frame = new ProximityBeaconFrame(Hex("02 15 0102"));

        Assert.False(frame.IsValid());
        Assert.Equal(Guid.Empty, frame.Uuid);
    }
}
