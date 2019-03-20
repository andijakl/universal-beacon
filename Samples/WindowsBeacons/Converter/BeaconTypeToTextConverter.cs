// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml.Data;
using UniversalBeacon.Library.Core.Entities;

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
