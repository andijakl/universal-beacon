// Copyright 2015 Andreas Jakl, Tieto Corporation. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// Based on the Google Eddystone specification, 
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

namespace UniversalBeaconLibrary.Beacon
{
    public static class BeaconFrameHelper
    {
        /// <summary>
        /// Number of bytes of the Eddystone header that is the same in all frame types.
        /// * 2 bytes for 0xAA, 0xFE to identify Eddystone.
        /// * 1 byte for the frame type.
        /// </summary>
        public const int EddystoneHeaderSize = 3;

        public enum EddystoneFrameType : byte
        {
            UidFrameType = 0x00,
            UrlFrameType = 0x10,
            TelemetryFrameType = 0x20
        }

        public static BeaconFrameBase CreateBeaconFrame(this byte[] payload)
        {
            if (!payload.IsEddystoneFrameType()) return null;
            switch (payload.GetEddystoneFrameType())
            {
                case EddystoneFrameType.UidFrameType:
                    return new UidEddystoneFrame(payload);
                case EddystoneFrameType.UrlFrameType:
                    return new UrlEddystoneFrame(payload);
                case EddystoneFrameType.TelemetryFrameType:
                    return new TlmEddystoneFrame(payload);
                case null:
                    return null;
                default:
                    return new UnknownBeaconFrame(payload);
            }
        }

        public static bool IsEddystoneFrameType(this byte[] payload)
        {
            if (payload == null || payload.Length < 3) return false;

            if (!(payload[0] == 0xAA && payload[1] == 0xFE)) return false;

            var frameTypeByte = payload[2];
            return Enum.IsDefined(typeof(EddystoneFrameType), frameTypeByte);
        }

        public static EddystoneFrameType? GetEddystoneFrameType(this byte[] payload)
        {
            if (!IsEddystoneFrameType(payload)) return null;
            return (EddystoneFrameType)payload[2];
        }

        public static byte[] CreateEddystoneHeader(EddystoneFrameType eddystoneType)
        {
            return new byte[] {0xAA, 0xFE, (byte)eddystoneType};
        }
    }
}
