namespace UniversalBeacon.Sample.Services;

/// <summary>
/// Requests the permissions required to scan for Bluetooth beacons.
/// </summary>
public static class BeaconPermissions
{
    /// <summary>
    /// Request the runtime permissions needed for scanning on the current platform.
    /// </summary>
    /// <returns>True if scanning is allowed.</returns>
    public static async Task<bool> RequestAsync()
    {
#if ANDROID
        // Android 12+: BLUETOOTH_SCAN, plus location - as the app does not declare "neverForLocation",
        // which would filter beacons from the scan results. Older versions: location only.
        var status = await Permissions.CheckStatusAsync<Platforms.Android.BeaconScanPermission>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Platforms.Android.BeaconScanPermission>();
        }
        return status == PermissionStatus.Granted;
#else
        // iOS / Mac Catalyst: the system shows the Bluetooth permission prompt automatically
        // (NSBluetoothAlwaysUsageDescription in Info.plist). If denied, the provider reports it
        // through the WatcherStopped event.
        // Windows: no runtime permission needed.
        await Task.CompletedTask;
        return true;
#endif
    }
}
