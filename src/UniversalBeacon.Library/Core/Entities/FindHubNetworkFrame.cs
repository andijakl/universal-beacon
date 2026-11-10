// Copyright 2026 Andreas Jakl. All rights reserved.
// https://github.com/andijakl/universal-beacon
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// Advertisement frame of an accessory (e.g., an item tracker) in Google's Find Hub Network
/// (formerly Find My Device Network), according to the
/// <see href="https://developers.google.com/nearby/fast-pair/specifications/extensions/fmdn">Find Hub Network Accessory Specification</see>.
///
/// These frames reuse the Eddystone service UUID 0xFEAA with frame type 0x40 or 0x41.
/// Layout: [0xAA 0xFE] [frame type] [ephemeral identifier: 20 or 32 bytes] [hashed flags: 1 byte, optional].
///
/// The ephemeral identifier rotates and can only be resolved by the owner of the accessory.
/// The hashed flags are obfuscated and can only be decoded with the ephemeral identity key, so
/// they are exposed as raw value only.
/// </summary>
public class FindHubNetworkFrame : BeaconFrameBase
{
    private const int EidOffset = BeaconFrameHelper.EddystoneHeaderSize;

    /// <summary>
    /// Length of the ephemeral identifier when the accessory uses the 160-bit elliptic curve.
    /// </summary>
    public const int ShortEphemeralIdentifierLength = 20;

    /// <summary>
    /// Length of the ephemeral identifier when the accessory uses the 256-bit elliptic curve.
    /// </summary>
    public const int LongEphemeralIdentifierLength = 32;

    private byte[]? _ephemeralIdentifier;
    private bool _isUnwantedTrackingProtectionModeActive;
    private byte? _hashedFlags;

    /// <summary>
    /// The rotating ephemeral identifier (20 or 32 bytes).
    /// </summary>
    public byte[]? EphemeralIdentifier => _ephemeralIdentifier;

    /// <summary>
    /// Ephemeral identifier formatted as upper-case hex string.
    /// </summary>
    public string EphemeralIdentifierAsString =>
        _ephemeralIdentifier == null ? string.Empty : ByteHelpers.ToHex(_ephemeralIdentifier, 0, _ephemeralIdentifier.Length);

    /// <summary>
    /// True if the accessory signals that it is in unwanted tracking protection mode (frame type 0x41).
    /// This mode is activated when the accessory is separated from its owner, to help
    /// detecting unknown trackers that are moving with you.
    /// </summary>
    public bool IsUnwantedTrackingProtectionModeActive => _isUnwantedTrackingProtectionModeActive;

    /// <summary>
    /// Raw (obfuscated) hashed flags byte, or null if the accessory does not send it.
    /// </summary>
    public byte? HashedFlags => _hashedFlags;

    /// <summary>
    /// Create new instance of the Find Hub Network frame based on the provided payload.
    /// </summary>
    /// <param name="payload">Payload to parse for this frame type.</param>
    public FindHubNetworkFrame(byte[] payload) : base(payload)
    {
        ParsePayload();
    }

    /// <summary>
    /// Parse the current payload into the properties exposed by this class.
    /// </summary>
    public void ParsePayload()
    {
        var payload = Payload;
        if (payload == null || !IsValid()) return;

        var newUtpMode = payload[2] == (byte)BeaconFrameHelper.EddystoneFrameType.FindHubUnwantedTrackingProtectionFrameType;
        if (newUtpMode != _isUnwantedTrackingProtectionModeActive)
        {
            _isUnwantedTrackingProtectionModeActive = newUtpMode;
            OnPropertyChanged(nameof(IsUnwantedTrackingProtectionModeActive));
        }

        var eidLength = GetEphemeralIdentifierLength(payload.Length);
        var newEid = ByteHelpers.Slice(payload, EidOffset, eidLength);
        if (!ByteHelpers.BytesEqual(newEid, _ephemeralIdentifier))
        {
            _ephemeralIdentifier = newEid;
            OnPropertyChanged(nameof(EphemeralIdentifier));
            OnPropertyChanged(nameof(EphemeralIdentifierAsString));
        }

        byte? newHashedFlags = payload.Length > EidOffset + eidLength ? payload[EidOffset + eidLength] : null;
        if (newHashedFlags != _hashedFlags)
        {
            _hashedFlags = newHashedFlags;
            OnPropertyChanged(nameof(HashedFlags));
        }
    }

    public override void Update(BeaconFrameBase otherFrame)
    {
        base.Update(otherFrame);
        ParsePayload();
    }

    /// <summary>
    /// Checks the header, the frame type and the frame length.
    /// </summary>
    public override bool IsValid()
    {
        var payload = Payload;
        if (payload == null) return false;
        var frameType = payload.GetEddystoneFrameType();
        if (frameType != BeaconFrameHelper.EddystoneFrameType.FindHubFrameType &&
            frameType != BeaconFrameHelper.EddystoneFrameType.FindHubUnwantedTrackingProtectionFrameType)
        {
            return false;
        }
        return GetEphemeralIdentifierLength(payload.Length) > 0;
    }

    private static int GetEphemeralIdentifierLength(int payloadLength)
    {
        switch (payloadLength - EidOffset)
        {
            case ShortEphemeralIdentifierLength:
            case ShortEphemeralIdentifierLength + 1:
                return ShortEphemeralIdentifierLength;
            case LongEphemeralIdentifierLength:
            case LongEphemeralIdentifierLength + 1:
                return LongEphemeralIdentifierLength;
            default:
                return 0;
        }
    }
}
