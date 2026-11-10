using System.Globalization;

namespace UniversalBeacon.Sample.Views;

/// <summary>
/// Displays a byte array as hex string, e.g. for the raw payload of unknown frames.
/// </summary>
public class BytesToHexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is byte[] bytes ? System.Convert.ToHexString(bytes) : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
