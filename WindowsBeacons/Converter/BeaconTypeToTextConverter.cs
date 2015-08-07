using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using UniversalBeaconLibrary.Beacon;

namespace WindowsBeacons.Converter
{
    public class BeaconTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Beacon.BeaconTypeEnum)
            {
                var beaconType = (Beacon.BeaconTypeEnum)value;
                switch (beaconType)
                {
                    case Beacon.BeaconTypeEnum.Unknown:
                        return "Unknown";
                    case Beacon.BeaconTypeEnum.Eddystone:
                        return "Eddystone";
                    case Beacon.BeaconTypeEnum.iBeacon:
                        return "iBeacon";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
