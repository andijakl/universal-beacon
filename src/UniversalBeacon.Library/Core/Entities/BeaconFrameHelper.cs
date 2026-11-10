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

namespace UniversalBeacon.Library.Core.Entities;

public static class BeaconFrameHelper
{
    /// <summary>
    /// Number of bytes of the Eddystone header that is the same in all frame types.
    /// * 2 bytes for 0xAA, 0xFE to identify Eddystone (16-bit service UUID 0xFEAA, little-endian).
    /// * 1 byte for the frame type.
    /// </summary>
    public const int EddystoneHeaderSize = 3;

    /// <summary>
    /// Frame types sent as service data of the Eddystone service UUID 0xFEAA.
    /// </summary>
    public enum EddystoneFrameType : byte
    {
        /// <summary>Eddystone-UID: static namespace + instance ID.</summary>
        UidFrameType = 0x00,
        /// <summary>Eddystone-URL: compressed URL (formerly used by the Physical Web).</summary>
        UrlFrameType = 0x10,
        /// <summary>Eddystone-TLM: telemetry. Version 0x00 = plain, 0x01 = encrypted (eTLM).</summary>
        TelemetryFrameType = 0x20,
        /// <summary>Eddystone-EID: rotating, encrypted ephemeral identifier.</summary>
        EidFrameType = 0x30,
        /// <summary>
        /// Google Find Hub Network (formerly Find My Device Network) accessory frame.
        /// Reserved in the original Eddystone specification.
        /// </summary>
        FindHubFrameType = 0x40,
        /// <summary>
        /// Google Find Hub Network accessory frame, sent while the accessory is in
        /// unwanted tracking protection mode.
        /// </summary>
        FindHubUnwantedTrackingProtectionFrameType = 0x41
    }

    /// <summary>
    /// Analyzes the payload of the Bluetooth Beacon frame and instantiates
    /// the according specialized Bluetooth frame class.
    /// Currently handles Eddystone frames and Find Hub Network frames, which share the
    /// Eddystone service UUID.
    /// </summary>
    /// <param name="payload">New payload to analyze, starting with the service UUID (0xAA 0xFE).</param>
    /// <returns>Base class for Bluetooth frames, which is either a specialized
    /// class or an UnknownBeaconFrame if the frame type is unknown or the frame is malformed.
    /// Returns null if the payload does not belong to the Eddystone service.</returns>
    public static BeaconFrameBase? CreateEddystoneBeaconFrame(this byte[]? payload)
    {
        if (payload == null || !payload.IsEddystoneFrameType()) return null;

        BeaconFrameBase frame;
        switch ((EddystoneFrameType)payload[2])
        {
            case EddystoneFrameType.UidFrameType:
                frame = new UidEddystoneFrame(payload);
                break;
            case EddystoneFrameType.UrlFrameType:
                frame = new UrlEddystoneFrame(payload);
                break;
            case EddystoneFrameType.TelemetryFrameType:
                frame = payload.Length > EddystoneHeaderSize &&
                        payload[EddystoneHeaderSize] == EncryptedTlmEddystoneFrame.EncryptedTlmVersion
                    ? new EncryptedTlmEddystoneFrame(payload)
                    : new TlmEddystoneFrame(payload);
                break;
            case EddystoneFrameType.EidFrameType:
                frame = new EidEddystoneFrame(payload);
                break;
            case EddystoneFrameType.FindHubFrameType:
            case EddystoneFrameType.FindHubUnwantedTrackingProtectionFrameType:
                frame = new FindHubNetworkFrame(payload);
                break;
            default:
                return new UnknownBeaconFrame(payload);
        }

        // Malformed frames (e.g., wrong length) are kept as unknown frames instead of
        // exposing garbage values.
        return frame.IsValid() ? frame : new UnknownBeaconFrame(payload);
    }

    /// <summary>
    /// Analyzes the header of the payload to check if the frame belongs to the
    /// Eddystone service. It needs to start with 0xAA 0xFE, followed by the frame type.
    ///
    /// Does not analyze if the frame type is known or if the rest of the contents are valid
    /// according to the specification - this task is up to the specialized handler classes.
    /// </summary>
    /// <param name="payload">Frame payload to analyze.</param>
    /// <returns>True if this is an Eddystone service frame, false if not.</returns>
    public static bool IsEddystoneFrameType(this byte[]? payload)
    {
        return payload != null && payload.Length >= EddystoneHeaderSize &&
               payload[0] == 0xAA && payload[1] == 0xFE;
    }

    /// <summary>
    /// Retrieve the Eddystone frame type for this frame.
    /// </summary>
    /// <param name="payload">Frame payload to analyze.</param>
    /// <returns>The Eddystone frame type byte. The value is not necessarily a defined
    /// member of <see cref="EddystoneFrameType"/>.
    /// If it is not an Eddystone frame, returns null.</returns>
    public static EddystoneFrameType? GetEddystoneFrameType(this byte[]? payload)
    {
        if (payload == null || !payload.IsEddystoneFrameType()) return null;
        return (EddystoneFrameType)payload[2];
    }

    /// <summary>
    /// Create the first three bytes of the Eddystone payload.
    /// 0xAA 0xFE [Eddystone type].
    /// </summary>
    /// <param name="eddystoneType">Eddystone type to use for this frame.</param>
    /// <returns>Byte array with the size of 3 containing the Eddystone frame header.</returns>
    public static byte[] CreateEddystoneHeader(EddystoneFrameType eddystoneType)
    {
        return [0xAA, 0xFE, (byte)eddystoneType];
    }

    /// <summary>
    /// Check if the beacon information corresponds to the Proximity Beacon type (compatible to iBeacon).
    /// </summary>
    /// <param name="companyId">Company ID of the beacon advertisement to check.</param>
    /// <param name="manufacturerData">Manufacturer data segment of the beacon advertisement to check.</param>
    /// <returns>True if the supplied data indicates that this is Proximity Beacon (iBeacon) frame data.</returns>
    public static bool IsProximityBeaconPayload(ushort companyId, byte[]? manufacturerData)
    {
        return companyId == ProximityBeaconFrame.CompanyId &&
               manufacturerData != null &&
               manufacturerData.Length >= ProximityBeaconFrame.PayloadLength &&
               manufacturerData[0] == ProximityBeaconFrame.DataType &&
               manufacturerData[1] == ProximityBeaconFrame.DataLength;
    }
}
