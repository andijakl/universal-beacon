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
/// Frame type for Proximity Beacon frames (compatible to iBeacon).
/// The payload is the manufacturer data of the advertisement without the company ID:
/// [0x02] [0x15] [UUID: 16 bytes] [Major: 2 bytes] [Minor: 2 bytes] [Tx power at 1 m: 1 byte].
/// All multi-byte values are big-endian.
/// </summary>
public class ProximityBeaconFrame : BeaconFrameBase
{
    /// <summary>
    /// Bluetooth company identifier of Apple Inc.
    /// </summary>
    public const ushort CompanyId = 0x004C;
    public const byte DataType = 0x02;
    public const byte DataLength = 0x15;

    /// <summary>
    /// Total length of the payload: data type + data length + 21 bytes of data.
    /// </summary>
    public const int PayloadLength = 2 + DataLength;

    private Guid _uuid;
    private ushort _major;
    private ushort _minor;
    private sbyte _txPower;

    /// <summary>
    /// Create a new frame by parsing the manufacturer data payload (without company ID).
    /// </summary>
    public ProximityBeaconFrame(byte[] payload)
        : base(payload)
    {
        ParsePayload();
    }

    /// <summary>
    /// Create a new frame based on the supplied values.
    /// </summary>
    public ProximityBeaconFrame(Guid uuid, ushort major, ushort minor, sbyte txPower)
    {
        _uuid = uuid;
        _major = major;
        _minor = minor;
        _txPower = txPower;
        UpdatePayload();
    }

    public string UuidAsString => Uuid.ToString("D");

    public Guid Uuid
    {
        get => _uuid;
        set
        {
            if (_uuid == value) return;
            _uuid = value;
            UpdatePayload();
            OnPropertyChanged();
            OnPropertyChanged(nameof(UuidAsString));
        }
    }

    public string MajorAsString => Major.ToString("x4");

    public ushort Major
    {
        get => _major;
        set
        {
            if (_major == value) return;
            _major = value;
            UpdatePayload();
            OnPropertyChanged();
            OnPropertyChanged(nameof(MajorAsString));
        }
    }

    public string MinorAsString => Minor.ToString("x4");

    public ushort Minor
    {
        get => _minor;
        set
        {
            if (_minor == value) return;
            _minor = value;
            UpdatePayload();
            OnPropertyChanged();
            OnPropertyChanged(nameof(MinorAsString));
        }
    }

    /// <summary>
    /// Calibrated signal strength in dBm at a distance of 1 m.
    /// </summary>
    public sbyte TxPower
    {
        get => _txPower;
        set
        {
            if (_txPower == value) return;
            _txPower = value;
            UpdatePayload();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Parse the current payload into the properties exposed by this class.
    /// Invalid payloads are ignored.
    /// </summary>
    public void ParsePayload()
    {
        var payload = Payload;
        if (payload == null || !IsValid()) return;

        var newUuid = ByteHelpers.GuidFromBigEndian(payload, 2);
        if (newUuid != _uuid)
        {
            _uuid = newUuid;
            OnPropertyChanged(nameof(Uuid));
            OnPropertyChanged(nameof(UuidAsString));
        }

        var newMajor = ByteHelpers.ReadUInt16BigEndian(payload, 18);
        if (newMajor != _major)
        {
            _major = newMajor;
            OnPropertyChanged(nameof(Major));
            OnPropertyChanged(nameof(MajorAsString));
        }

        var newMinor = ByteHelpers.ReadUInt16BigEndian(payload, 20);
        if (newMinor != _minor)
        {
            _minor = newMinor;
            OnPropertyChanged(nameof(Minor));
            OnPropertyChanged(nameof(MinorAsString));
        }

        var newTxPower = (sbyte)payload[22];
        if (newTxPower != _txPower)
        {
            _txPower = newTxPower;
            OnPropertyChanged(nameof(TxPower));
        }
    }

    private void UpdatePayload()
    {
        var payload = new byte[PayloadLength];
        payload[0] = DataType;
        payload[1] = DataLength;
        Buffer.BlockCopy(ByteHelpers.GuidToBigEndian(_uuid), 0, payload, 2, 16);
        ByteHelpers.WriteUInt16BigEndian(payload, 18, _major);
        ByteHelpers.WriteUInt16BigEndian(payload, 20, _minor);
        payload[22] = (byte)_txPower;
        _payload = payload;
    }

    public override void Update(BeaconFrameBase otherFrame)
    {
        base.Update(otherFrame);
        ParsePayload();
    }

    /// <summary>
    /// Checks the data type, data length and payload length.
    /// </summary>
    public override bool IsValid()
    {
        var payload = Payload;
        return payload != null &&
               payload.Length >= PayloadLength &&
               payload[0] == DataType &&
               payload[1] == DataLength;
    }
}
