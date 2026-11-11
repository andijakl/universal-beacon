using System.Globalization;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Sample.Views;

/// <summary>
/// A few well-known Bluetooth identifiers, to make the raw advertisement data easier to read.
/// See the Bluetooth SIG "Assigned Numbers" for the complete lists.
/// </summary>
internal static class KnownBluetoothIds
{
    public static readonly Dictionary<ushort, string> Companies = new()
    {
        [0x0006] = "Microsoft",
        [0x004C] = "Apple",
        [0x0059] = "Nordic Semiconductor",
        [0x0075] = "Samsung",
        [0x0087] = "Garmin",
        [0x009E] = "Bose",
        [0x00E0] = "Google",
        [0x012D] = "Sony",
        [0x0499] = "Ruuvi Innovations",
    };

    public static readonly Dictionary<uint, string> Services = new()
    {
        [0x180A] = "Device Information",
        [0x180F] = "Battery",
        [0x1812] = "Human Interface Device",
        [0xFD6F] = "Exposure Notification",
        [0xFE2C] = "Google Fast Pair",
        [0xFEAA] = "Eddystone",
    };
}

/// <summary>
/// Formats a company identifier, e.g. "0x004C (Apple)".
/// </summary>
public class CompanyIdToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ushort companyId) return string.Empty;
        var text = "0x" + companyId.ToString("X4", CultureInfo.InvariantCulture);
        return KnownBluetoothIds.Companies.TryGetValue(companyId, out var name) ? $"{text} ({name})" : text;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Formats a service UUID in its short form, e.g. "0xFE2C (Google Fast Pair)".
/// </summary>
public class ServiceUuidToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Guid uuid) return string.Empty;
        var text = BluetoothUuids.ToShortString(uuid);
        return BluetoothUuids.TryGetShortUuid(uuid, out var shortUuid) &&
               KnownBluetoothIds.Services.TryGetValue(shortUuid, out var name)
            ? $"{text} ({name})"
            : text;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// True if the value is a non-empty string, e.g. to hide labels without content.
/// </summary>
public class IsNotEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && text.Length > 0;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
