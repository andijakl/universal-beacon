using Android;

namespace UniversalBeacon.Sample.Platforms.Android;

/// <summary>
/// Runtime permissions required for scanning Bluetooth beacons on Android.
/// </summary>
public class BeaconScanPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(31)
            ?
            [
                (Manifest.Permission.BluetoothScan, true),
                (Manifest.Permission.AccessFineLocation, true)
            ]
            :
            [
                (Manifest.Permission.AccessFineLocation, true)
            ];
}
