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
using System.Linq;

namespace UniversalBeaconLibrary.Beacon
{
    public class UidEddystoneFrame : BeaconFrameBase
    {
        private byte _rangingData;

        public byte RangingData
        {
            get { return _rangingData; }
            set
            {
                if (_rangingData == value) return;
                _rangingData = value;
                UpdatePayload();
                OnPropertyChanged();
            }
        }

        private byte[] _namespaceId;

        public byte[] NamespaceId
        {
            get { return _namespaceId; }
            set
            {
                if (_namespaceId == value) return;
                if (value == null)
                {
                    _namespaceId = null;
                    return;
                }
                if (_namespaceId != null && _namespaceId.SequenceEqual(value)) return;
                _namespaceId = new byte[value.Length];
                Array.Copy(value, _namespaceId, value.Length);
                UpdatePayload();
                OnPropertyChanged();
            }
        }

        private byte[] _instanceId;

        public byte[] InstanceId
        {
            get { return _instanceId; }
            set
            {
                if (value == null)
                {
                    _instanceId = null;
                    return;
                }
                if (_instanceId != null && _instanceId.SequenceEqual(value)) return;
                _instanceId = new byte[value.Length];
                Array.Copy(value, _instanceId, value.Length);
                UpdatePayload();
                OnPropertyChanged();
            }
        }

        public UidEddystoneFrame(byte[] payload) : base(payload)
        {
            ParsePayload();
        }

        public void ParsePayload()
        {
            if (!IsValid()) return;

            // The Ranging Data is the Tx power in dBm emitted by the beacon at 0 meters.
            // Note that this is different from other beacon protocol specifications that require the Tx power 
            // to be measured at 1 m.The value is an 8-bit integer as specified by the Tx Power Level Characteristic 
            // and ranges from -100 dBm to +20 dBm to a resolution of 1 dBm.
            RangingData = Payload[BeaconFrameHelper.EddystoneHeaderSize];

            // Namespace ID
            var nId = new byte[10];
            Array.Copy(Payload, BeaconFrameHelper.EddystoneHeaderSize + 1, nId, 0, 10);
            _namespaceId = nId;

            // Instance ID
            var iId = new byte[6];
            Array.Copy(Payload, BeaconFrameHelper.EddystoneHeaderSize + 11, iId, 0, 6);
            _instanceId = iId;

            //Debug.WriteLine("Eddystone Uid Frame: Ranging data = "
            //    + RangingData + ", NS = " + BitConverter.ToString(NamespaceId) + ", Instance = " + BitConverter.ToString(InstanceId));
        }

        private void UpdatePayload()
        {
            // TODO 
        }
        public override void Update(BeaconFrameBase otherFrame)
        {
            ParsePayload();
        }

        public override bool IsValid()
        {
            if (!base.IsValid()) return false;

            // 2 bytes ID: AA FE
            // 1 byte frame type
            if (!Payload.IsEddystoneFrameType()) return false;

            // 1 byte ranging data
            // 10 bytes namespace id
            // 6 bytes instance id
            return Payload.Length == BeaconFrameHelper.EddystoneHeaderSize + 17;

        }
    }
}
