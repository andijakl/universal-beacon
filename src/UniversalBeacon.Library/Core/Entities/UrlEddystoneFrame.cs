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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniversalBeacon.Library.Core.Entities;

/// <summary>
/// An Eddystone URL frame, according to the Google Specification from
/// https://github.com/google/eddystone/tree/master/eddystone-url
///
/// Note: the Physical Web project and Google's Nearby Notifications that consumed
/// these frames were discontinued; the frame format itself is still in use.
/// </summary>
public class UrlEddystoneFrame : BeaconFrameBase
{
    private const int UrlSchemeOffset = BeaconFrameHelper.EddystoneHeaderSize + 1;
    private const int EncodedUrlOffset = UrlSchemeOffset + 1;

    private static readonly string[] UrlCodes =
    [
        ".com/", ".org/", ".edu/", ".net/", ".info/", ".biz/", ".gov/",
        ".com", ".org", ".edu", ".net", ".info", ".biz", ".gov"
    ];

    private static readonly string[] UrlSchemes =
    [
        "http://www.", "https://www.", "http://", "https://"
    ];

    private sbyte _rangingData;
    private string? _completeUrl;

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
    /// The complete URL, including the scheme prefix (e.g., https://).
    /// </summary>
    public string? CompleteUrl
    {
        get => _completeUrl;
        set
        {
            if (_completeUrl == value) return;
            _completeUrl = value;
            UpdatePayload();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Create new URL Eddystone frame based on the provided data.
    /// </summary>
    /// <param name="rangingData">Ranging data (Tx power level) - see property documentation.</param>
    /// <param name="completeUrl">Complete URL, which has to start with one of the schemes supported
    /// by Eddystone (http://www., https://www., http://, https://).</param>
    public UrlEddystoneFrame(sbyte rangingData, string? completeUrl)
    {
        _rangingData = rangingData;
        _completeUrl = completeUrl;
        UpdatePayload();
    }

    /// <summary>
    /// Create new instance of the URL Eddystone frame based on the provided payload.
    /// Parses the payload and initializes the instance.
    /// </summary>
    /// <param name="payload">Payload to parse for this frame type.</param>
    public UrlEddystoneFrame(byte[] payload) : base(payload)
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

        var newRangingData = (sbyte)payload[BeaconFrameHelper.EddystoneHeaderSize];
        if (newRangingData != _rangingData)
        {
            _rangingData = newRangingData;
            OnPropertyChanged(nameof(RangingData));
        }

        var newCompleteUrl = UrlSchemePrefixAsString(payload[UrlSchemeOffset]) + DecodeUrl(payload, EncodedUrlOffset);
        if (newCompleteUrl != _completeUrl)
        {
            _completeUrl = newCompleteUrl;
            OnPropertyChanged(nameof(CompleteUrl));
        }
    }

    /// <summary>
    /// Update the raw payload when properties have changed.
    /// </summary>
    private void UpdatePayload()
    {
        var url = _completeUrl;
        var urlScheme = url == null ? null : EncodeUrlScheme(url);
        if (url == null || urlScheme == null)
        {
            // Only URLs starting with one of the defined schemes can be encoded.
            _payload = null;
            return;
        }

        using var ms = new MemoryStream();
        var header = BeaconFrameHelper.CreateEddystoneHeader(BeaconFrameHelper.EddystoneFrameType.UrlFrameType);
        ms.Write(header, 0, header.Length);
        ms.WriteByte((byte)_rangingData);
        ms.WriteByte(urlScheme.Value);
        EncodeUrlToStream(url, UrlSchemes[urlScheme.Value].Length, ms);
        _payload = ms.ToArray();
    }

    private static byte? EncodeUrlScheme(string url)
    {
        // Check longer schemes first, so that "http://www." wins over "http://".
        for (byte i = 0; i < UrlSchemes.Length; i++)
        {
            if (url.StartsWith(UrlSchemes[i], StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return null;
    }

    private static void EncodeUrlToStream(string url, int pos, Stream ms)
    {
        while (pos < url.Length)
        {
            var code = FindUrlCode(url, pos);
            if (code == null)
            {
                // No expansion code at this position - write the character itself.
                var curCharBytes = Encoding.ASCII.GetBytes(url.Substring(pos, 1));
                ms.Write(curCharBytes, 0, curCharBytes.Length);
                pos++;
            }
            else
            {
                ms.WriteByte(code.Value);
                pos += UrlCodes[code.Value].Length;
            }
        }
    }

    /// <summary>
    /// Find the longest expansion code that matches the URL at the given position.
    /// </summary>
    private static byte? FindUrlCode(string url, int pos)
    {
        byte? result = null;
        var expansionLength = 0;
        for (byte i = 0; i < UrlCodes.Length; i++)
        {
            var code = UrlCodes[i];
            if (code.Length > expansionLength &&
                string.CompareOrdinal(url, pos, code, 0, code.Length) == 0)
            {
                expansionLength = code.Length;
                result = i;
            }
        }
        return result;
    }

    private static string DecodeUrl(byte[] rawUrl, int startAtArrayPos)
    {
        var sb = new StringBuilder(rawUrl.Length);
        for (var i = startAtArrayPos; i < rawUrl.Length; i++)
        {
            var b = rawUrl[i];
            if (b < UrlCodes.Length)
            {
                sb.Append(UrlCodes[b]);
            }
            else if (b > 0x20 && b < 0x7F)
            {
                sb.Append((char)b);
            }
            // 0x0E..0x20 and 0x7F..0xFF are reserved by the specification and skipped.
        }
        return sb.ToString();
    }

    /// <summary>
    /// Convert the URL scheme byte to its string representation.
    /// </summary>
    /// <param name="urlSchemePrefix">URL scheme prefix byte (0x00 - 0x03).</param>
    /// <returns>The scheme prefix, or null if the byte is not a defined scheme.</returns>
    public string? UrlSchemePrefixAsString(byte urlSchemePrefix)
    {
        return urlSchemePrefix < UrlSchemes.Length ? UrlSchemes[urlSchemePrefix] : null;
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
    /// Checks the header, the URL scheme as well as the minimum frame length.
    /// </summary>
    /// <returns>True if the frame is a valid Eddystone URL frame.</returns>
    public override bool IsValid()
    {
        var payload = Payload;
        if (payload == null) return false;
        if (payload.GetEddystoneFrameType() != BeaconFrameHelper.EddystoneFrameType.UrlFrameType) return false;

        // 1 byte ranging data, 1 byte url scheme prefix
        return payload.Length >= EncodedUrlOffset && payload[UrlSchemeOffset] < UrlSchemes.Length;
    }
}
