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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UniversalBeaconLibrary.Beacon
{
    public class UrlEddystoneFrame : BeaconFrameBase
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

        private string _completeUrl;
        public string CompleteUrl
        {
            get { return _completeUrl; }
            set
            {
                if (_completeUrl == value) return;
                _completeUrl = value;
                UpdatePayload();
                OnPropertyChanged();
            }
        }

        private static readonly Dictionary<byte, string> UrlCodeDictionary = new Dictionary<byte, string>
        {
            {0, ".com/"},
            {1, ".org/"},
            {2, ".edu/"},
            {3, ".net/"},
            {4, ".info/"},
            {5, ".biz/"},
            {6, ".gov/"},
            {7, ".com"},
            {8, ".org"},
            {9, ".edu"},
            {10, ".net"},
            {11, ".info"},
            {12, ".biz"},
            {13, ".gov"},
        };

        private static readonly Dictionary<byte, string> UrlSchemeDictionary = new Dictionary<byte, string>
        {
            {0, "http://www."},
            {1, "https://www."},
            {2, "http://"},
            {3, "https://"}
        };

        public UrlEddystoneFrame(byte rangingData, string completeUrl)
        {
            _rangingData = rangingData;
            _completeUrl = completeUrl;
            UpdatePayload();
        }

        public UrlEddystoneFrame(byte[] payload) : base(payload)
        {
            ParsePayload();
        }

        public void ParsePayload()
        {
            if (!IsValid()) return;

            // Ranging data
            var newRangingData = Payload[BeaconFrameHelper.EddystoneHeaderSize];
            if (newRangingData != RangingData)
            {
                _rangingData = newRangingData;
                OnPropertyChanged(nameof(RangingData));
            }

            // URL Scheme prefix (1 byte)
            var urlSchemePrefix = Payload[BeaconFrameHelper.EddystoneHeaderSize + 1];
            
            // Decode complete URL
            var newCompleteUrl = UrlSchemePrefixAsString(urlSchemePrefix) + DecodeUrl(Payload, BeaconFrameHelper.EddystoneHeaderSize + 2);
            if (newCompleteUrl != CompleteUrl)
            {
                _completeUrl = newCompleteUrl;
                OnPropertyChanged(nameof(CompleteUrl));
            }

            //Debug.WriteLine("Eddystone URL Frame: Url = " + CompleteUrl);
        }

        private void UpdatePayload()
        {
            if (string.IsNullOrEmpty(CompleteUrl))
            {
                _payload = null;
                return;
            }
            var urlSchemeByte = EncodeUrlScheme(CompleteUrl);
            // Check if the URL starts with a valid header (only the defined ones are allowed)
            if (urlSchemeByte == null)
            {
                _payload = null;
                return;
            }

            var header = BeaconFrameHelper.CreateEddystoneHeader(BeaconFrameHelper.EddystoneFrameType.UrlFrameType);
            using (var ms = new MemoryStream())
            {
                // Frame header
                ms.Write(header, 0, header.Length);
                // Ranging data
                ms.WriteByte(RangingData);
                // URL scheme byte
                ms.WriteByte((byte)urlSchemeByte);
                // Encoded URL
                EncodeUrlToStream(CompleteUrl, UrlSchemeDictionary[(byte) urlSchemeByte].Length, ms);
                // Save to payload (to direct array to prevent re-parsing and a potential endless loop of updating and parsing)
                _payload = ms.ToArray();
            }
        }

        private static byte? EncodeUrlScheme(string url)
        {
            foreach (var curScheme in UrlSchemeDictionary)
            {
                if (url.StartsWith(curScheme.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return curScheme.Key;
                }
            }
            return null;
        }

        private void EncodeUrlToStream(string url, int pos, Stream ms)
        {
            // Encode the rest of the URL string
            while(pos < url.Length)
            {
                var code = FindUrlCode(url.Substring(pos));
                if (code == null)
                {
                    // Didn't find URL code at this position
                    var curCharByte = Encoding.ASCII.GetBytes(new[] {url[pos]});
                    ms.Write(curCharByte, 0, curCharByte.Length);
                    pos++;
                }
                else
                {
                    // Write URL code
                    ms.WriteByte((byte)code);
                    pos += UrlCodeDictionary[(byte) code].Length;
                }
            }
        }

        private byte? FindUrlCode(string url)
        {
            var dictPos = -1;
            var expansionLength = 0;

            for (var i = 0; i < UrlCodeDictionary.Count; i++)
            {
                if (UrlCodeDictionary[(byte)i].Length > expansionLength)
                {
                    if (url.StartsWith(UrlCodeDictionary[(byte)i]))
                    {
                        expansionLength = UrlCodeDictionary[(byte)i].Length;
                        dictPos = i;
                    }
                }
            }
            
            if (dictPos >= 0)
                return (byte)dictPos;
            return null;
        }


        private static string DecodeUrl(byte[] rawUrl, int startAtArrayPos)
        {
            if (rawUrl.Length < startAtArrayPos) return null;
            var sb = new StringBuilder(rawUrl.Length);
            for(var i = startAtArrayPos; i < rawUrl.Length; i++)
            {
                sb.Append(UrlCodeDictionary.ContainsKey(rawUrl[i])
                    ? UrlCodeDictionary[rawUrl[i]]
                    : Encoding.UTF8.GetString(new[] { rawUrl[i] }));
            }
            return sb.ToString();
        }

        public string UrlSchemePrefixAsString(byte urlSchemePrefix)
        {
            return UrlSchemeDictionary.ContainsKey(urlSchemePrefix) ? UrlSchemeDictionary[urlSchemePrefix] : null;
        }

        public override void Update(BeaconFrameBase otherFrame)
        {
            base.Update(otherFrame);
            ParsePayload();
        }

        public override bool IsValid()
        {
            if (!base.IsValid()) return false;

            // 2 bytes ID: AA FE
            // 1 byte frame type
            if (!Payload.IsEddystoneFrameType()) return false;

            // 1 byte ranging data
            // 1 byte url scheme prefix
            return Payload.Length >= BeaconFrameHelper.EddystoneHeaderSize + 2;
        }
    }
}
