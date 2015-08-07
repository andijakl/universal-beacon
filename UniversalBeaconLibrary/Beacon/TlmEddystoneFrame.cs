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
using System.Diagnostics;
using System.IO;

namespace UniversalBeaconLibrary.Beacon
{
    public class TlmEddystoneFrame : BeaconFrameBase
    {
        private byte _version;
        private ushort _batteryInMilliV;
        private float _temperatureInC;
        private uint _advertisementFrameCount;
        private uint _timeSincePowerUp;

        public byte Version
        {
            get { return _version; }
            set
            {
                if (_version == value) return;
                _version = value;
                UpdatePayload();
            }
        }

        public ushort BatteryInMilliV
        {
            get { return _batteryInMilliV; }
            set
            {
                if (_batteryInMilliV == value) return;
                _batteryInMilliV = value;
                UpdatePayload();
            }
        }

        public float TemperatureInC
        {
            get { return _temperatureInC; }
            set
            {
                if (Math.Abs(_temperatureInC - value) < 0.0001) return;
                _temperatureInC = value;
                UpdatePayload();
            }
        }

        public uint AdvertisementFrameCount
        {
            get { return _advertisementFrameCount; }
            set
            {
                if (_advertisementFrameCount == value) return;
                _advertisementFrameCount = value;
                UpdatePayload();
            }
        }

        public uint TimeSincePowerUp
        {
            get { return _timeSincePowerUp; }
            set
            {
                if (_timeSincePowerUp == value) return;
                _timeSincePowerUp = value;
                UpdatePayload();
            }
        }


        public TlmEddystoneFrame(byte[] payload) : base(payload)
        {
            ParsePayload();
        }

        public void ParsePayload()
        {
            using (var ms = new MemoryStream(Payload, false))
            {
                using (var reader = new BinaryReader(ms))
                {
                    // Skip frame header
                    ms.Position = BeaconFrameHelper.EddystoneHeaderSize;

                    // At present the value must be  0x00 
                    _version = reader.ReadByte();

                    // Battery voltage is the current battery charge in millivolts, expressed as 1 mV per bit.
                    // If not supported (for example in a USB-powered beacon) the value should be zeroed.
                    var batteryBytes = reader.ReadBytes(2);
                    Array.Reverse(batteryBytes);
                    _batteryInMilliV = BitConverter.ToUInt16(batteryBytes, 0);

                    // Beacon temperature is the temperature in degrees Celsius sensed by the beacon and 
                    // expressed in a signed 8.8 fixed-point notation. 
                    // If not supported the value should be set to 0x8000, -128 °C.
                    // use signed https://courses.cit.cornell.edu/ee476/Math/
                    // #define float2fix(a) ((int)((a)*256.0))         //Convert float to fix. a is a float
                    // #define fix2float(a) ((float)(a)/256.0)         //Convert fix to float. a is an int 
                    var temperatureBytes = reader.ReadBytes(2);
                    Array.Reverse(temperatureBytes);
                    _temperatureInC = BitConverter.ToInt16(temperatureBytes, 0) / (float)256.0;

                    // ADV_CNT is the running count of advertisement frames of all types 
                    // emitted by the beacon since power-up or reboot, useful for monitoring 
                    // performance metrics that scale per broadcast frame.
                    // If this value is reset (e.g.on reboot), the current time field must also be reset.
                    var advCntBytes = reader.ReadBytes(4);
                    Array.Reverse(advCntBytes);
                    _advertisementFrameCount = BitConverter.ToUInt32(advCntBytes, 0);

                    // SEC_CNT is a 0.1 second resolution counter that represents time since beacon power - 
                    // up or reboot. If this value is reset (e.g.on a reboot), the ADV count field must also be reset.
                    var secCntBytes = reader.ReadBytes(4);
                    Array.Reverse(secCntBytes);
                    _timeSincePowerUp = BitConverter.ToUInt32(secCntBytes, 0);
                    
                    //Debug.WriteLine("Eddystone Tlm Frame: Version = "
                    //    + Version + ", Battery = " + (BatteryInMilliV / 1000.0) + "V, Temperature = " + TemperatureInC
                    //    + "°C, Frame count = " + AdvertisementFrameCount + ", Time since power up = " + TimeSincePowerUp);
                }
            }
        }


        private void UpdatePayload()
        {
            // TODO
        }

        public override bool IsValid()
        {
            if (!base.IsValid()) return false;

            // 2 bytes ID: AA FE
            // 1 byte frame type
            if (!Payload.IsEddystoneFrameType()) return false;

            // 1 byte version
            // 2 bytes battery voltage
            // 2 bytes beacon temperature
            // 4 bytes adv_cnt (AdvertisementFrameCount)
            // 4 bytes sec_cnt (TimeSincePowerUp)
            return Payload.Length == BeaconFrameHelper.EddystoneHeaderSize + 13;
        }
    }
}
