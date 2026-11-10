// Copyright 2015 - 2026 Andreas Jakl. All rights reserved.
// https://github.com/andijakl/universal-beacon
//
// Based on the Eddystone specification by Google,
// available under Apache License, Version 2.0 from
// https://github.com/google/eddystone
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

using System;

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// An Eddystone EID frame, according to the Google Specification from
/// https://github.com/google/eddystone/tree/master/eddystone-eid
///
/// The ephemeral identifier rotates periodically and can only be resolved to a
/// beacon identity with the beacon's identity key.
/// </summary>
public class EidEddystoneFrame : BeaconFrameBase
{
    private const int EphemeralIdentifierLength = 8;
    private const int EphemeralIdentifierOffset = BeaconFrameHelper.EddystoneHeaderSize + 1;
    private const int PayloadLength = EphemeralIdentifierOffset + EphemeralIdentifierLength;

    private sbyte _rangingData;
    private byte[]? _ephemeralIdentifier;

    /// <summary>
    /// Tx power level - the received power level at 0 m, in dBm.
    /// Values range from -100 to +20 dBM, with a resolution of 1 dBm.
    /// </summary>
    public sbyte RangingData
    {
        get => _rangingData;
        set
        {
            if (_rangingData == value) return;
            _rangingData = value;
            UpdatePayload();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 8-byte Ephemeral Identifier, used for cryptography.
    /// For hints how to compute the EID, see:
    /// https://github.com/google/eddystone/blob/master/eddystone-eid/eid-computation.md
    /// </summary>
    public byte[]? EphemeralIdentifier
    {
        get => _ephemeralIdentifier;
        set
        {
            if (value is not { Length: EphemeralIdentifierLength }) value = null;
            if (ByteHelpers.BytesEqual(_ephemeralIdentifier, value)) return;
            _ephemeralIdentifier = ByteHelpers.CopyOrNull(value);
            UpdatePayload();
            OnPropertyChanged();
            OnPropertyChanged(nameof(EphemeralIdentifierAsString));
        }
    }

    /// <summary>
    /// Ephemeral identifier formatted as upper-case hex string.
    /// </summary>
    public string EphemeralIdentifierAsString =>
        EphemeralIdentifier == null ? string.Empty : ByteHelpers.ToHex(EphemeralIdentifier, 0, EphemeralIdentifier.Length);

    /// <summary>
    /// Create new instance of the EID Eddystone frame based on the provided payload.
    /// Parses the payload and initializes the instance.
    /// </summary>
    /// <param name="payload">Payload to parse for this frame type.</param>
    public EidEddystoneFrame(byte[] payload) : base(payload)
    {
        ParsePayload();
    }

    /// <summary>
    /// Create new EID Eddystone frame based on the provided data.
    /// </summary>
    /// <param name="rangingData">Ranging data (Tx power level) - see property documentation.</param>
    /// <param name="ephemeralIdentifier">Ephemeral Identifier (8 bytes) - see property documentation.</param>
    public EidEddystoneFrame(sbyte rangingData, byte[]? ephemeralIdentifier)
    {
        _rangingData = rangingData;
        if (ephemeralIdentifier is { Length: EphemeralIdentifierLength })
        {
            _ephemeralIdentifier = ByteHelpers.CopyOrNull(ephemeralIdentifier);
        }
        UpdatePayload();
    }

    /// <summary>
    /// Parse the current payload into the properties exposed by this class.
    /// Has to be called if manually modifying the raw payload.
    /// </summary>
    public void ParsePayload()
    {
        var payload = Payload;
        if (payload == null || !IsValid()) return;

        var newRangingData = (sbyte)payload[BeaconFrameHelper.EddystoneHeaderSize];
        if (newRangingData != _rangingData)
        {
            _rangingData = newRangingData;
            OnPropertyChanged(nameof(RangingData));
        }

        var newEphemeralIdentifier = ByteHelpers.Slice(payload, EphemeralIdentifierOffset, EphemeralIdentifierLength);
        if (!ByteHelpers.BytesEqual(newEphemeralIdentifier, _ephemeralIdentifier))
        {
            _ephemeralIdentifier = newEphemeralIdentifier;
            OnPropertyChanged(nameof(EphemeralIdentifier));
            OnPropertyChanged(nameof(EphemeralIdentifierAsString));
        }
    }

    /// <summary>
    /// Update the raw payload when properties have changed.
    /// </summary>
    private void UpdatePayload()
    {
        if (_ephemeralIdentifier is not { Length: EphemeralIdentifierLength })
        {
            _payload = null;
            return;
        }

        var payload = new byte[PayloadLength];
        Buffer.BlockCopy(BeaconFrameHelper.CreateEddystoneHeader(BeaconFrameHelper.EddystoneFrameType.EidFrameType),
            0, payload, 0, BeaconFrameHelper.EddystoneHeaderSize);
        payload[BeaconFrameHelper.EddystoneHeaderSize] = (byte)_rangingData;
        Buffer.BlockCopy(_ephemeralIdentifier, 0, payload, EphemeralIdentifierOffset, EphemeralIdentifierLength);
        _payload = payload;
    }

    /// <summary>
    /// Update the information stored in this frame with the information from the other frame.
    /// </summary>
    /// <param name="otherFrame">Frame to use as source for updating the information in this beacon
    /// frame.</param>
    public override void Update(BeaconFrameBase otherFrame)
    {
        base.Update(otherFrame);
        ParsePayload();
    }

    /// <summary>
    /// Check if the contents of this frame are generally valid.
    /// Checks the header as well as the minimum frame length.
    /// </summary>
    /// <returns>True if the frame is a valid Eddystone EID frame.</returns>
    public override bool IsValid()
    {
        var payload = Payload;
        if (payload == null) return false;
        if (payload.GetEddystoneFrameType() != BeaconFrameHelper.EddystoneFrameType.EidFrameType) return false;

        // 1 byte ranging data, 8 bytes ephemeral identifier.
        // Some beacons append 0-padding, so longer frames are accepted.
        return payload.Length >= PayloadLength;
    }
}
