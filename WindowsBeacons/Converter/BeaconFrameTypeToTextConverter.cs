using System;
using Windows.UI.Xaml.Data;
using UniversalBeaconLibrary.Beacon;

namespace WindowsBeacons.Converter
{
    public class BeaconFrameTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UnknownBeaconFrame)
                return "Unknown Frame";
            if (value is TlmEddystoneFrame)
                return "Telemetry Frame";
            if (value is UidEddystoneFrame)
                return "UID Frame";
            if (value is UrlEddystoneFrame)
                return "URL Frame";

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
