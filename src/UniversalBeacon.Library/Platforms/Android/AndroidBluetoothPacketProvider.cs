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
using System.Collections.Generic;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library;

/// <summary>
/// Bluetooth LE advertisement provider for Android, based on the BluetoothLeScanner.
///
/// Your app needs to declare and request the permissions to scan for Bluetooth devices:
/// on Android 12+ BLUETOOTH_SCAN (without the neverForLocation flag, which would filter beacons)
/// plus ACCESS_FINE_LOCATION; on older versions ACCESS_FINE_LOCATION.
/// </summary>
public sealed class AndroidBluetoothPacketProvider : IBluetoothPacketProvider, IDisposable
{
    public event EventHandler<BLEAdvertisementPacketArgs>? AdvertisementPacketReceived;
    public event EventHandler<BTError>? WatcherStopped;

    private readonly BluetoothAdapter? _adapter;
    private readonly BLEScanCallback _scanCallback;
    private readonly object _lock = new();
    private BluetoothLeScanner? _scanner;
    private bool _disposed;

    /// <summary>
    /// Scan mode used when starting the scan. Defaults to low latency, which
    /// is best suited for apps in the foreground.
    /// </summary>
    public Android.Bluetooth.LE.ScanMode ScanMode { get; set; } = Android.Bluetooth.LE.ScanMode.LowLatency;

    /// <summary>
    /// True while the scan is running.
    /// </summary>
    public bool IsScanning => _scanner != null;

    /// <param name="context">Android context, e.g., the application context.</param>
    public AndroidBluetoothPacketProvider(Context context)
    {
        var manager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        _adapter = manager?.Adapter;
        _scanCallback = new BLEScanCallback();
        _scanCallback.AdvertisementPacketReceived += ScanCallback_AdvertisementPacketReceived;
        _scanCallback.ScanFailed += ScanCallback_ScanFailed;
    }

    private void ScanCallback_AdvertisementPacketReceived(object? sender, BLEAdvertisementPacketArgs e)
    {
        AdvertisementPacketReceived?.Invoke(this, e);
    }

    private void ScanCallback_ScanFailed(object? sender, ScanFailure e)
    {
        lock (_lock)
        {
            _scanner = null;
        }
        var error = e == ScanFailure.FeatureUnsupported
            ? BTError.BluetoothError.NotSupported
            : BTError.BluetoothError.OtherError;
        WatcherStopped?.Invoke(this, new BTError(error));
    }

    /// <summary>
    /// Start scanning for Bluetooth LE advertisements.
    /// If Bluetooth is not available or turned off, or if the app is missing the required permission,
    /// the WatcherStopped event is raised with the according error code.
    /// </summary>
    public void Start()
    {
        BTError.BluetoothError? error = null;
        lock (_lock)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AndroidBluetoothPacketProvider));
            if (_scanner != null) return;

            var scanner = _adapter is { IsEnabled: true } ? _adapter.BluetoothLeScanner : null;
            if (scanner == null)
            {
                error = BTError.BluetoothError.RadioNotAvailable;
            }
            else
            {
                try
                {
                    var settings = new ScanSettings.Builder()
                        .SetScanMode(ScanMode)!
                        .Build();
                    scanner.StartScan(new List<ScanFilter>(), settings, _scanCallback);
                    _scanner = scanner;
                }
                catch (Java.Lang.SecurityException)
                {
                    error = BTError.BluetoothError.ConsentRequired;
                }
            }
        }

        if (error != null)
        {
            WatcherStopped?.Invoke(this, new BTError(error.Value));
        }
    }

    /// <summary>
    /// Stop scanning for Bluetooth LE advertisements.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (_scanner == null) return;
            try
            {
                _scanner.StopScan(_scanCallback);
            }
            catch (Java.Lang.IllegalStateException)
            {
                // Bluetooth was turned off in the meantime - the scan is stopped anyway.
            }
            catch (Java.Lang.SecurityException)
            {
                // Permission was revoked in the meantime.
            }
            _scanner = null;
        }
        WatcherStopped?.Invoke(this, new BTError(BTError.BluetoothError.Success));
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _disposed = true;
        _scanCallback.AdvertisementPacketReceived -= ScanCallback_AdvertisementPacketReceived;
        _scanCallback.ScanFailed -= ScanCallback_ScanFailed;
        _scanCallback.Dispose();
    }
}
