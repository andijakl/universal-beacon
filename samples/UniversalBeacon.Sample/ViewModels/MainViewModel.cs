using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;
using UniversalBeacon.Sample.Services;

namespace UniversalBeacon.Sample.ViewModels;

public partial class MainViewModel : ObservableObject
{
    /// <summary>
    /// Devices that have not been seen for this time are removed from the list.
    /// Most phones, laptops and headphones change their Bluetooth address regularly for privacy,
    /// so without removing them, the list would keep growing.
    /// </summary>
    private static readonly TimeSpan StaleDeviceTimeout = TimeSpan.FromMinutes(1);

    private readonly BeaconManager _beaconManager;
    private readonly IDispatcherTimer? _staleDeviceTimer;
    private readonly HashSet<Beacon> _trackedBeacons = [];

    public MainViewModel(BeaconManager beaconManager, IBluetoothPacketProvider provider)
    {
        _beaconManager = beaconManager;
        provider.WatcherStopped += (_, error) => MainThread.BeginInvokeOnMainThread(() => OnWatcherStopped(error));

        // The beacon manager modifies its collection on the UI thread (see MauiProgram).
        _beaconManager.BluetoothBeacons.CollectionChanged += OnBeaconsCollectionChanged;

        _staleDeviceTimer = Dispatcher.GetForCurrentThread()?.CreateTimer();
        if (_staleDeviceTimer != null)
        {
            _staleDeviceTimer.Interval = TimeSpan.FromSeconds(10);
            _staleDeviceTimer.Tick += (_, _) => _beaconManager.RemoveStaleBeacons(StaleDeviceTimeout);
        }
    }

    /// <summary>
    /// Devices shown in the list - all devices, or only recognized beacons.
    /// </summary>
    public ObservableCollection<Beacon> VisibleBeacons { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartStopText))]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Tap Start to scan for Bluetooth beacons.";

    /// <summary>
    /// Only show devices recognized as beacons (Eddystone, iBeacon, Find Hub accessories).
    /// </summary>
    [ObservableProperty]
    public partial bool ShowBeaconsOnly { get; set; }

    public string StartStopText => IsScanning ? "Stop" : "Start";

    [RelayCommand]
    private async Task StartStopAsync()
    {
        if (IsScanning)
        {
            _beaconManager.Stop();
            SetScanning(false);
            Status = "Stopped.";
            return;
        }

        if (!await BeaconPermissions.RequestAsync())
        {
            Status = "Permission to scan for Bluetooth devices was denied. On Android, allow nearby devices and precise location.";
            return;
        }

        SetScanning(true);
        Status = $"Scanning… Devices not seen for {StaleDeviceTimeout.TotalSeconds:0} seconds are removed.";
        _beaconManager.Start();
    }

    [RelayCommand]
    private void Clear() => _beaconManager.Clear();

    partial void OnShowBeaconsOnlyChanged(bool value) => RebuildVisibleBeacons();

    private void SetScanning(bool scanning)
    {
        IsScanning = scanning;
        // Keep the list as it is while not scanning.
        if (scanning) _staleDeviceTimer?.Start();
        else _staleDeviceTimer?.Stop();
    }

    private bool IsVisible(Beacon beacon) =>
        !ShowBeaconsOnly || beacon.BeaconType != Beacon.BeaconTypeEnum.Unknown;

    private void OnBeaconsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (Beacon beacon in e.NewItems!)
                {
                    Track(beacon);
                    if (IsVisible(beacon)) VisibleBeacons.Add(beacon);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (Beacon beacon in e.OldItems!)
                {
                    Untrack(beacon);
                    VisibleBeacons.Remove(beacon);
                }
                break;
            default:
                RebuildVisibleBeacons();
                break;
        }
    }

    private void RebuildVisibleBeacons()
    {
        foreach (var beacon in _trackedBeacons.ToList()) Untrack(beacon);
        VisibleBeacons.Clear();
        foreach (var beacon in _beaconManager.BluetoothBeacons)
        {
            Track(beacon);
            if (IsVisible(beacon)) VisibleBeacons.Add(beacon);
        }
    }

    private void Track(Beacon beacon)
    {
        if (_trackedBeacons.Add(beacon)) beacon.PropertyChanged += OnBeaconPropertyChanged;
    }

    private void Untrack(Beacon beacon)
    {
        if (_trackedBeacons.Remove(beacon)) beacon.PropertyChanged -= OnBeaconPropertyChanged;
    }

    private void OnBeaconPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // A device can be recognized as beacon after its first advertisement.
        if (e.PropertyName != nameof(Beacon.BeaconType) || sender is not Beacon beacon) return;
        if (IsVisible(beacon) && !VisibleBeacons.Contains(beacon)) VisibleBeacons.Add(beacon);
    }

    private void OnWatcherStopped(BTError error)
    {
        if (error.BluetoothErrorCode == BTError.BluetoothError.Success) return;

        SetScanning(false);
        Status = error.BluetoothErrorCode switch
        {
            BTError.BluetoothError.RadioNotAvailable => "Bluetooth is turned off or not available.",
            BTError.BluetoothError.ConsentRequired or BTError.BluetoothError.DisabledByUser =>
                "Access to Bluetooth was denied. Please allow it in the system settings.",
            BTError.BluetoothError.DisabledByPolicy => "Bluetooth is disabled by policy.",
            BTError.BluetoothError.NotSupported => "Bluetooth LE scanning is not supported on this device.",
            _ => $"Scanning stopped: {error.BluetoothErrorCode}"
        };
    }
}
