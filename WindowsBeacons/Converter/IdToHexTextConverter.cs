using System;
using System.Numerics;
using Windows.UI.Xaml.Data;

namespace WindowsBeacons.Converter
{
    public class IdToHexTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[])
            {
                return BitConverter.ToString((byte[])value);
            }
            if (value is ulong)
            {
                return ((ulong) value).ToString("X");
            }
            if (value is BigInteger)
            {
                return ((BigInteger) value).ToString("X");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
