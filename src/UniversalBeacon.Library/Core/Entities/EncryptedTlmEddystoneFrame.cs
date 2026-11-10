// Copyright 2026 Andreas Jakl. All rights reserved.
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

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// An encrypted Eddystone Telemetry frame (eTLM, TLM version 0x01), according to
/// https://github.com/google/eddystone/blob/master/eddystone-tlm/tlm-encrypted.md
///
/// The telemetry data is encrypted with AES-EAX using the beacon's identity key and
/// can only be decrypted in combination with the EID broadcast of the same beacon.
/// This class exposes the raw encrypted data, salt and message integrity check.
/// </summary>
public class EncryptedTlmEddystoneFrame : BeaconFrameBase
{
    /// <summary>
    /// TLM version for encrypted telemetry data.
    /// </summary>
    public const byte EncryptedTlmVersion = 0x01;

    private const int VersionOffset = BeaconFrameHelper.EddystoneHeaderSize;
    private const int EncryptedDataOffset = VersionOffset + 1;
    private const int EncryptedDataLength = 12;
    private const int SaltOffset = EncryptedDataOffset + EncryptedDataLength;
    private const int MicOffset = SaltOffset + 2;
    private const int PayloadLength = MicOffset + 2;

    private byte[]? _encryptedData;
    private ushort _salt;
    private ushort _messageIntegrityCheck;

    /// <summary>
    /// 12 bytes of AES-EAX encrypted telemetry data.
    /// </summary>
    public byte[]? EncryptedData => _encryptedData;

    /// <summary>
    /// Encrypted data formatted as upper-case hex string.
    /// </summary>
    public string EncryptedDataAsString =>
        _encryptedData == null ? string.Empty : ByteHelpers.ToHex(_encryptedData, 0, _encryptedData.Length);

    /// <summary>
    /// 16-bit random salt, used as part of the nonce.
    /// </summary>
    public ushort Salt => _salt;

    /// <summary>
    /// 16-bit message integrity check (AES-EAX tag).
    /// </summary>
    public ushort MessageIntegrityCheck => _messageIntegrityCheck;

    /// <summary>
    /// Create new instance of the encrypted TLM frame based on the provided payload.
    /// </summary>
    /// <param name="payload">Payload to parse for this frame type.</param>
    public EncryptedTlmEddystoneFrame(byte[] payload) : base(payload)
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

        var newEncryptedData = ByteHelpers.Slice(payload, EncryptedDataOffset, EncryptedDataLength);
        if (!ByteHelpers.BytesEqual(newEncryptedData, _encryptedData))
        {
            _encryptedData = newEncryptedData;
            OnPropertyChanged(nameof(EncryptedData));
            OnPropertyChanged(nameof(EncryptedDataAsString));
        }

        var newSalt = ByteHelpers.ReadUInt16BigEndian(payload, SaltOffset);
        if (newSalt != _salt)
        {
            _salt = newSalt;
            OnPropertyChanged(nameof(Salt));
        }

        var newMic = ByteHelpers.ReadUInt16BigEndian(payload, MicOffset);
        if (newMic != _messageIntegrityCheck)
        {
            _messageIntegrityCheck = newMic;
            OnPropertyChanged(nameof(MessageIntegrityCheck));
        }
    }

    public override void Update(BeaconFrameBase otherFrame)
    {
        base.Update(otherFrame);
        ParsePayload();
    }

    /// <summary>
    /// Checks the header, the TLM version (0x01) and the frame length.
    /// </summary>
    public override bool IsValid()
    {
        var payload = Payload;
        if (payload == null) return false;
        if (payload.GetEddystoneFrameType() != BeaconFrameHelper.EddystoneFrameType.TelemetryFrameType) return false;
        return payload.Length == PayloadLength && payload[VersionOffset] == EncryptedTlmVersion;
    }
}
