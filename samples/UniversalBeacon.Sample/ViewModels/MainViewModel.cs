using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;
using UniversalBeacon.Sample.Services;

namespace UniversalBeacon.Sample.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly BeaconManager _beaconManager;

    public MainViewModel(BeaconManager beaconManager, IBluetoothPacketProvider provider)
    {
        _beaconManager = beaconManager;
        provider.WatcherStopped += (_, error) => MainThread.BeginInvokeOnMainThread(() => OnWatcherStopped(error));
    }

    /// <summary>
    /// Beacons found so far. Updated on the UI thread by the beacon manager.
    /// </summary>
    public ObservableCollection<Beacon> Beacons => _beaconManager.BluetoothBeacons;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartStopText))]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Tap Start to scan for Bluetooth beacons.";

    public string StartStopText => IsScanning ? "Stop" : "Start";

    [RelayCommand]
    private async Task StartStopAsync()
    {
        if (IsScanning)
        {
            _beaconManager.Stop();
            IsScanning = false;
            Status = "Stopped.";
            return;
        }

        if (!await BeaconPermissions.RequestAsync())
        {
            Status = "Permission to scan for Bluetooth devices was denied. On Android, allow nearby devices and precise location.";
            return;
        }

        IsScanning = true;
        Status = "Scanning for beacons…";
        _beaconManager.Start();
    }

    [RelayCommand]
    private void Clear() => _beaconManager.Clear();

    private void OnWatcherStopped(BTError error)
    {
        if (error.BluetoothErrorCode == BTError.BluetoothError.Success) return;

        IsScanning = false;
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
