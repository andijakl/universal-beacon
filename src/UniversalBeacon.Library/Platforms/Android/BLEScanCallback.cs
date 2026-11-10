// Copyright 2015 - 2026 Andreas Jakl, Chris Tacke. All rights reserved.
// https://github.com/andijakl/universal-beacon
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
using Android.Bluetooth.LE;
using Android.OS;
using Android.Runtime;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library;

/// <summary>
/// Receives the scan results from Android and converts them to platform-independent packets.
/// </summary>
internal sealed class BLEScanCallback : ScanCallback
{
    public event EventHandler<BLEAdvertisementPacketArgs>? AdvertisementPacketReceived;
    public event EventHandler<ScanFailure>? ScanFailed;

    public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
    {
        base.OnScanFailed(errorCode);
        ScanFailed?.Invoke(this, errorCode);
    }

    public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult? result)
    {
        base.OnScanResult(callbackType, result);
        var packet = ToPacket(result);
        if (packet != null)
        {
            AdvertisementPacketReceived?.Invoke(this, new BLEAdvertisementPacketArgs(packet));
        }
    }

    public override void OnBatchScanResults(System.Collections.Generic.IList<ScanResult>? results)
    {
        base.OnBatchScanResults(results);
        if (results == null) return;
        foreach (var result in results)
        {
            OnScanResult(ScanCallbackType.AllMatches, result);
        }
    }

    private static BLEAdvertisementPacket? ToPacket(ScanResult? result)
    {
        var address = result?.Device?.Address;
        if (result == null || address == null) return null;

        var scanRecord = result.ScanRecord;
        var advertisement = BLEAdvertisement.Parse(scanRecord?.GetBytes());
        advertisement.LocalName ??= scanRecord?.DeviceName;

        // TimestampNanos is based on the elapsed time since boot - convert to wall clock time.
        var ageInNanos = SystemClock.ElapsedRealtimeNanos() - result.TimestampNanos;
        var timestamp = DateTimeOffset.Now - TimeSpan.FromTicks(Math.Max(0, ageInNanos / 100));

        var connectable = !OperatingSystem.IsAndroidVersionAtLeast(26) || result.IsConnectable;

        return new BLEAdvertisementPacket
        {
            BluetoothAddress = address.ToNumericAddress(),
            RawSignalStrengthInDBm = (short)result.Rssi,
            Timestamp = timestamp,
            AdvertisementType = connectable
                ? BLEAdvertisementType.ConnectableUndirected
                : BLEAdvertisementType.NonConnectableUndirected,
            Advertisement = advertisement
        };
    }
}
