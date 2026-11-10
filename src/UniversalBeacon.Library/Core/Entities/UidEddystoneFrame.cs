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
using System.Numerics;

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// An Eddystone UID frame, according to the Google Specification from
/// https://github.com/google/eddystone/tree/master/eddystone-uid
/// </summary>
public class UidEddystoneFrame : BeaconFrameBase
{
    private const int NamespaceIdLength = 10;
    private const int InstanceIdLength = 6;
    private const int NamespaceIdOffset = BeaconFrameHelper.EddystoneHeaderSize + 1;
    private const int InstanceIdOffset = NamespaceIdOffset + NamespaceIdLength;

    /// <summary>
    /// Length without the two reserved (RFU) bytes, which are omitted by some beacons.
    /// </summary>
    private const int MinimumPayloadLength = InstanceIdOffset + InstanceIdLength;

    /// <summary>
    /// Length including the two reserved (RFU) bytes, as required by the specification.
    /// </summary>
    private const int FullPayloadLength = MinimumPayloadLength + 2;

    private sbyte _rangingData;
    private byte[]? _namespaceId;
    private byte[]? _instanceId;

    /// <summary>
    /// Tx power level - the received power level at 0 m, in dBm.
    /// Values range from -100 to +20 dBM, with a resolution of 1 dBm.
    /// In order to convert constants at compile time, you might need to run the conversion in
    /// an unchecked() context. For example:
    /// sbyte rangingData = unchecked((sbyte)0xEE);
    /// According to the documentation, this corresponds to the dBm value of -18.
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
    /// 10-byte namespace, intended to ensure ID uniqueness across multiple
    /// Eddystone implementers.
    /// For hints how to construct the namespace ID, see:
    /// https://github.com/google/eddystone/tree/master/eddystone-uid
    /// </summary>
    public byte[]? NamespaceId
    {
        get => _namespaceId;
        set
        {
            if (ByteHelpers.BytesEqual(_namespaceId, value)) return;
            _namespaceId = ByteHelpers.CopyOrNull(value);
            UpdatePayload();
            OnPropertyChanged();
            OnPropertyChanged(nameof(NamespaceIdAsNumber));
            OnPropertyChanged(nameof(NamespaceIdAsString));
        }
    }

    /// <summary>
    /// Interprets the namespace ID as unsigned big-endian number.
    /// As it is 10 bytes, returned as BigInteger.
    /// </summary>
    public BigInteger NamespaceIdAsNumber => ToUnsignedBigInteger(NamespaceId);

    /// <summary>
    /// Namespace ID formatted as upper-case hex string (20 characters).
    /// </summary>
    public string NamespaceIdAsString =>
        NamespaceId == null ? string.Empty : ByteHelpers.ToHex(NamespaceId, 0, NamespaceId.Length);

    /// <summary>
    /// 6-byte instance ID of the specific beacon belonging to a certain
    /// namespace.
    /// The ID can be assigned by your app in any custom way.
    /// </summary>
    public byte[]? InstanceId
    {
        get => _instanceId;
        set
        {
            if (ByteHelpers.BytesEqual(_instanceId, value)) return;
            _instanceId = ByteHelpers.CopyOrNull(value);
            UpdatePayload();
            OnPropertyChanged();
            OnPropertyChanged(nameof(InstanceIdAsNumber));
            OnPropertyChanged(nameof(InstanceIdAsString));
        }
    }

    /// <summary>
    /// Return the instance ID as unsigned big-endian number.
    /// </summary>
    public ulong InstanceIdAsNumber
    {
        get
        {
            if (InstanceId == null) return 0;
            ulong result = 0;
            foreach (var b in InstanceId)
            {
                result = (result << 8) | b;
            }
            return result;
        }
    }

    /// <summary>
    /// Instance ID formatted as upper-case hex string (12 characters).
    /// </summary>
    public string InstanceIdAsString =>
        InstanceId == null ? string.Empty : ByteHelpers.ToHex(InstanceId, 0, InstanceId.Length);

    /// <summary>
    /// Create new UID Eddystone frame based on the provided data.
    /// </summary>
    /// <param name="rangingData">Ranging data (Tx power level) - see property documentation.</param>
    /// <param name="namespaceId">Namespace ID (10 bytes) - see property documentation.</param>
    /// <param name="instanceId">Instance ID (6 bytes) - see property documentation.</param>
    public UidEddystoneFrame(sbyte rangingData, byte[]? namespaceId, byte[]? instanceId)
    {
        _rangingData = rangingData;
        if (namespaceId is { Length: NamespaceIdLength })
        {
            _namespaceId = ByteHelpers.CopyOrNull(namespaceId);
        }
        if (instanceId is { Length: InstanceIdLength })
        {
            _instanceId = ByteHelpers.CopyOrNull(instanceId);
        }
        UpdatePayload();
    }

    /// <summary>
    /// Create new instance of the UID Eddystone frame based on the provided payload.
    /// Parses the payload and initializes the instance.
    /// </summary>
    /// <param name="payload">Payload to parse for this frame type.</param>
    public UidEddystoneFrame(byte[] payload) : base(payload)
    {
        ParsePayload();
    }

    /// <summary>
    /// Parse the current payload into the properties exposed by this class.
    /// Has to be called if manually modifying the raw payload.
    /// </summary>
    public void ParsePayload()
    {
        var payload = Payload;
        if (payload == null || !IsValid()) return;

        // The Ranging Data is the Tx power in dBm emitted by the beacon at 0 meters.
        // Note that this is different from other beacon protocol specifications that require the Tx power
        // to be measured at 1 m. The value is an 8-bit integer as specified by the Tx Power Level Characteristic
        // and ranges from -100 dBm to +20 dBm to a resolution of 1 dBm.
        var newRangingData = (sbyte)payload[BeaconFrameHelper.EddystoneHeaderSize];
        if (newRangingData != _rangingData)
        {
            _rangingData = newRangingData;
            OnPropertyChanged(nameof(RangingData));
        }

        var newNamespaceId = ByteHelpers.Slice(payload, NamespaceIdOffset, NamespaceIdLength);
        if (!ByteHelpers.BytesEqual(newNamespaceId, _namespaceId))
        {
            _namespaceId = newNamespaceId;
            OnPropertyChanged(nameof(NamespaceId));
            OnPropertyChanged(nameof(NamespaceIdAsNumber));
            OnPropertyChanged(nameof(NamespaceIdAsString));
        }

        var newInstanceId = ByteHelpers.Slice(payload, InstanceIdOffset, InstanceIdLength);
        if (!ByteHelpers.BytesEqual(newInstanceId, _instanceId))
        {
            _instanceId = newInstanceId;
            OnPropertyChanged(nameof(InstanceId));
            OnPropertyChanged(nameof(InstanceIdAsNumber));
            OnPropertyChanged(nameof(InstanceIdAsString));
        }
    }

    /// <summary>
    /// Update the raw payload when properties have changed.
    /// </summary>
    private void UpdatePayload()
    {
        if (_namespaceId is not { Length: NamespaceIdLength } ||
            _instanceId is not { Length: InstanceIdLength })
        {
            _payload = null;
            return;
        }

        var payload = new byte[FullPayloadLength];
        Buffer.BlockCopy(BeaconFrameHelper.CreateEddystoneHeader(BeaconFrameHelper.EddystoneFrameType.UidFrameType),
            0, payload, 0, BeaconFrameHelper.EddystoneHeaderSize);
        payload[BeaconFrameHelper.EddystoneHeaderSize] = (byte)_rangingData;
        Buffer.BlockCopy(_namespaceId, 0, payload, NamespaceIdOffset, NamespaceIdLength);
        Buffer.BlockCopy(_instanceId, 0, payload, InstanceIdOffset, InstanceIdLength);
        // The last two bytes are reserved (RFU) and must be 0x00.

        // Save directly to the field to prevent re-parsing.
        _payload = payload;
    }

    /// <summary>
    /// Update the information stored in this frame with the information from the other frame.
    /// Useful for example when binding the UI to beacon information, as this will emit
    /// property changed notifications whenever a value changes - which would not be possible if
    /// you would overwrite the whole frame.
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
    /// Checks the header as well as the frame length.
    /// </summary>
    /// <returns>True if the frame is a valid Eddystone UID frame.</returns>
    public override bool IsValid()
    {
        var payload = Payload;
        if (payload == null) return false;
        if (payload.GetEddystoneFrameType() != BeaconFrameHelper.EddystoneFrameType.UidFrameType) return false;

        // 1 byte ranging data, 10 bytes namespace id, 6 bytes instance id,
        // 2 bytes RFU - have to be present according to the specification, but are
        // sometimes omitted in practice.
        return payload.Length == MinimumPayloadLength || payload.Length == FullPayloadLength;
    }

    private static BigInteger ToUnsignedBigInteger(byte[]? bigEndian)
    {
        if (bigEndian == null) return BigInteger.Zero;
        // BigInteger expects little-endian two's complement - append 0x00 to keep it positive.
        var littleEndian = new byte[bigEndian.Length + 1];
        for (var i = 0; i < bigEndian.Length; i++)
        {
            littleEndian[i] = bigEndian[bigEndian.Length - 1 - i];
        }
        return new BigInteger(littleEndian);
    }
}
