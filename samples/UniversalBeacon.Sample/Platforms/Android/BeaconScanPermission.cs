using Android;

namespace UniversalBeacon.Sample.Platforms.Android;

/// <summary>
/// Runtime permissions required for scanning Bluetooth beacons on Android.
/// </summary>
/// <remarks>
/// Fine location must be requested together with coarse location - some Android 12 releases
/// ignore a request for fine location alone. Scanning for beacons requires fine (precise) location,
/// so the request only counts as granted if the user allows precise location.
/// </remarks>
public class BeaconScanPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(31)
            ?
            [
                (Manifest.Permission.BluetoothScan, true),
                (Manifest.Permission.AccessFineLocation, true),
                (Manifest.Permission.AccessCoarseLocation, true)
            ]
            :
            [
                (Manifest.Permission.AccessFineLocation, true),
                (Manifest.Permission.AccessCoarseLocation, true)
            ];
}
