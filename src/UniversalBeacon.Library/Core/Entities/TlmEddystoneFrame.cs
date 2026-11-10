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
/// An unencrypted Eddystone Telemetry frame (version 0x00), according to the Google Specification from
/// https://github.com/google/eddystone/tree/master/eddystone-tlm
///
/// Encrypted telemetry frames (version 0x01) are represented by <see cref="EncryptedTlmEddystoneFrame"/>.
/// </summary>
public class TlmEddystoneFrame : BeaconFrameBase
{
    /// <summary>
    /// TLM version for unencrypted telemetry data.
    /// </summary>
    public const byte UnencryptedTlmVersion = 0x00;

    private const int VersionOffset = BeaconFrameHelper.EddystoneHeaderSize;
    private const int BatteryOffset = VersionOffset + 1;
    private const int TemperatureOffset = BatteryOffset + 2;
    private const int AdvertisementCountOffset = TemperatureOffset + 2;
    private const int TimeSincePowerUpOffset = AdvertisementCountOffset + 4;
    private const int PayloadLength = TimeSincePowerUpOffset + 4;

    private byte _version;
    private ushort _batteryInMilliV;
    private float _temperatureInC;
    private uint _advertisementFrameCount;
    private uint _timeSincePowerUp;

    /// <summary>
    /// Version of the Eddystone Telemetry (TLM) frame.
    /// Supported value for this class is 0x00 (unencrypted).
    /// </summary>
    public byte Version
    {
        get => _version;
        set
        {
            if (_version == value) return;
            _version = value;
            UpdatePayload();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The current battery voltage in millivolts [mV].
    /// If not supported (e.g., USB powered beacon) = 0.
    /// </summary>
    public ushort BatteryInMilliV
    {
        get => _batteryInMilliV;
        set
        {
            if (_batteryInMilliV == value) return;
            _batteryInMilliV = value;
            UpdatePayload();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Beacon temperature in degrees Celsius.
    /// Encoded as signed 8.8 fixed-point number - resolution is 1/256 °C.
    /// If not supported, -128 °C.
    /// </summary>
    public float TemperatureInC
    {
        get => _temperatureInC;
        set
        {
            if (Math.Abs(_temperatureInC - value) < 0.0001) return;
            _temperatureInC = value;
            UpdatePayload();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Running count of advertisement frames of all types emitted
    /// by the beacon since the last power up / reboot.
    /// </summary>
    public uint AdvertisementFrameCount
    {
        get => _advertisementFrameCount;
        set
        {
            if (_advertisementFrameCount == value) return;
            _advertisementFrameCount = value;
            UpdatePayload();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Time in 0.1 second resolution since the last beacon power up / reboot.
    /// </summary>
    public uint TimeSincePowerUp
    {
        get => _timeSincePowerUp;
        set
        {
            if (_timeSincePowerUp == value) return;
            _timeSincePowerUp = value;
            UpdatePayload();
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimeSincePowerUpAsTimeSpan));
        }
    }

    /// <summary>
    /// Time since the last beacon power up / reboot.
    /// </summary>
    public TimeSpan TimeSincePowerUpAsTimeSpan => TimeSpan.FromMilliseconds(TimeSincePowerUp * 100.0);

    /// <summary>
    /// Create new TLM Eddystone frame based on the provided data.
    /// </summary>
    public TlmEddystoneFrame(byte version, ushort batteryInMv, float temperatureInC,
        uint advertisementFrameCount, uint timeSincePowerUp)
    {
        _version = version;
        _batteryInMilliV = batteryInMv;
        _temperatureInC = temperatureInC;
        _advertisementFrameCount = advertisementFrameCount;
        _timeSincePowerUp = timeSincePowerUp;
        UpdatePayload();
    }

    /// <summary>
    /// Create new instance of the TLM Eddystone frame based on the provided payload.
    /// Parses the payload and initializes the instance.
    /// </summary>
    /// <param name="payload">Payload to parse for this frame type.</param>
    public TlmEddystoneFrame(byte[] payload) : base(payload)
    {
        ParsePayload();
    }

    /// <summary>
    /// Parse the current payload into the properties exposed by this class.
    /// Has to be called if manually modifying the raw payload.
    /// Invalid payloads (e.g., wrong length or encrypted TLM) are ignored.
    /// </summary>
    public void ParsePayload()
    {
        var payload = Payload;
        if (payload == null || !IsValid()) return;

        var newVersion = payload[VersionOffset];
        if (newVersion != _version)
        {
            _version = newVersion;
            OnPropertyChanged(nameof(Version));
        }

        // Battery voltage is the current battery charge in millivolts, expressed as 1 mV per bit.
        // If not supported (for example in a USB-powered beacon) the value should be zeroed.
        var newBatteryInMilliV = ByteHelpers.ReadUInt16BigEndian(payload, BatteryOffset);
        if (newBatteryInMilliV != _batteryInMilliV)
        {
            _batteryInMilliV = newBatteryInMilliV;
            OnPropertyChanged(nameof(BatteryInMilliV));
        }

        // Beacon temperature is the temperature in degrees Celsius sensed by the beacon and
        // expressed in a signed 8.8 fixed-point notation.
        // If not supported the value should be set to 0x8000, -128 °C.
        var rawTemperature = (short)ByteHelpers.ReadUInt16BigEndian(payload, TemperatureOffset);
        var newTemperatureInC = rawTemperature / 256.0f;
        if (Math.Abs(newTemperatureInC - _temperatureInC) > 0.0001)
        {
            _temperatureInC = newTemperatureInC;
            OnPropertyChanged(nameof(TemperatureInC));
        }

        // ADV_CNT is the running count of advertisement frames of all types emitted by the beacon
        // since power-up or reboot, useful for monitoring performance metrics that scale per broadcast frame.
        var newAdvertisementFrameCount = ByteHelpers.ReadUInt32BigEndian(payload, AdvertisementCountOffset);
        if (newAdvertisementFrameCount != _advertisementFrameCount)
        {
            _advertisementFrameCount = newAdvertisementFrameCount;
            OnPropertyChanged(nameof(AdvertisementFrameCount));
        }

        // SEC_CNT is a 0.1 second resolution counter that represents time since beacon power-up or reboot.
        var newTimeSincePowerUp = ByteHelpers.ReadUInt32BigEndian(payload, TimeSincePowerUpOffset);
        if (newTimeSincePowerUp != _timeSincePowerUp)
        {
            _timeSincePowerUp = newTimeSincePowerUp;
            OnPropertyChanged(nameof(TimeSincePowerUp));
            OnPropertyChanged(nameof(TimeSincePowerUpAsTimeSpan));
        }
    }

    /// <summary>
    /// Update the raw payload when properties have changed.
    /// </summary>
    private void UpdatePayload()
    {
        var payload = new byte[PayloadLength];
        Buffer.BlockCopy(BeaconFrameHelper.CreateEddystoneHeader(BeaconFrameHelper.EddystoneFrameType.TelemetryFrameType),
            0, payload, 0, BeaconFrameHelper.EddystoneHeaderSize);
        payload[VersionOffset] = _version;
        ByteHelpers.WriteUInt16BigEndian(payload, BatteryOffset, _batteryInMilliV);
        // Signed 8.8 fixed-point; the cast to ushort keeps the two's complement representation.
        var fixedPointTemperature = Math.Max(short.MinValue, Math.Min(short.MaxValue, Math.Round(_temperatureInC * 256.0)));
        ByteHelpers.WriteUInt16BigEndian(payload, TemperatureOffset, unchecked((ushort)(short)fixedPointTemperature));
        ByteHelpers.WriteUInt32BigEndian(payload, AdvertisementCountOffset, _advertisementFrameCount);
        ByteHelpers.WriteUInt32BigEndian(payload, TimeSincePowerUpOffset, _timeSincePowerUp);

        // Save directly to the field to prevent re-parsing.
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
    /// Checks the header, the TLM version as well as the frame length.
    /// </summary>
    /// <returns>True if the frame is a valid unencrypted Eddystone TLM frame.</returns>
    public override bool IsValid()
    {
        var payload = Payload;
        if (payload == null) return false;
        if (payload.GetEddystoneFrameType() != BeaconFrameHelper.EddystoneFrameType.TelemetryFrameType) return false;

        // 1 byte version, 2 bytes battery voltage, 2 bytes beacon temperature,
        // 4 bytes adv_cnt (AdvertisementFrameCount), 4 bytes sec_cnt (TimeSincePowerUp)
        return payload.Length == PayloadLength && payload[VersionOffset] == UnencryptedTlmVersion;
    }
}
