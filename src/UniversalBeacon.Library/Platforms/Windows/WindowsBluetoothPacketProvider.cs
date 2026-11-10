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
using Windows.Devices.Bluetooth.Advertisement;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library;

/// <summary>
/// Bluetooth LE advertisement provider for Windows 10 / 11, based on the
/// BluetoothLEAdvertisementWatcher. Works in WinUI 3 / .NET MAUI, WPF, WinForms and console apps.
///
/// Packaged apps need to declare the "bluetooth" device capability in their app manifest.
/// </summary>
public sealed class WindowsBluetoothPacketProvider : IBluetoothPacketProvider, IDisposable
{
    public event EventHandler<BLEAdvertisementPacketArgs>? AdvertisementPacketReceived;
    public event EventHandler<BTError>? WatcherStopped;

    private readonly BluetoothLEAdvertisementWatcher _watcher;
    private readonly object _lock = new();
    private bool _running;
    private bool _disposed;

    public WindowsBluetoothPacketProvider()
    {
        _watcher = new BluetoothLEAdvertisementWatcher
        {
            // Active scanning also receives scan responses, which often contain the device name.
            ScanningMode = BluetoothLEScanningMode.Active
        };
        _watcher.Received += WatcherOnReceived;
        _watcher.Stopped += WatcherOnStopped;
    }

    /// <summary>
    /// Underlying Windows advertisement watcher, e.g., to configure filters or the scanning mode.
    /// </summary>
    public BluetoothLEAdvertisementWatcher AdvertisementWatcher => _watcher;

    /// <summary>
    /// Current status of the watcher.
    /// </summary>
    public BLEAdvertisementWatcherStatusCodes WatcherStatus => (BLEAdvertisementWatcherStatusCodes)_watcher.Status;

    private void WatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
    {
        AdvertisementPacketReceived?.Invoke(this, new BLEAdvertisementPacketArgs(eventArgs.ToUniversalBLEPacket()));
    }

    private void WatcherOnStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
    {
        lock (_lock)
        {
            // Stopped either through Stop() or by the system (e.g., Bluetooth turned off).
            _running = false;
        }
        WatcherStopped?.Invoke(this, new BTError((BTError.BluetoothError)args.Error));
    }

    /// <summary>
    /// Start watching for Bluetooth LE advertisements.
    /// If Bluetooth is not available, the WatcherStopped event is raised with the according error code.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WindowsBluetoothPacketProvider));
            if (_running) return;
            _running = true;
            _watcher.Start();
        }
    }

    /// <summary>
    /// Stop watching for Bluetooth LE advertisements.
    /// The WatcherStopped event is raised once the watcher has stopped.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_running) return;
            _watcher.Stop();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _disposed = true;
        _watcher.Received -= WatcherOnReceived;
        _watcher.Stopped -= WatcherOnStopped;
    }
}
