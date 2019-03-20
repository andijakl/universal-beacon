// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using UniversalBeacon.Library.Core.Entities;

namespace WindowsBeacons.Converter
{
    public class BeaconFrameTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            if (value is UnknownBeaconFrame)
                return resourceLoader.GetString("UnknownBeaconFrame");
            if (value is TlmEddystoneFrame)
                return resourceLoader.GetString("TlmEddystoneFrame");
            if (value is UidEddystoneFrame)
                return resourceLoader.GetString("UidEddystoneFrame");
            if (value is UrlEddystoneFrame)
                return resourceLoader.GetString("UrlEddystoneFrame");
            if (value is EidEddystoneFrame)
                return resourceLoader.GetString("EidEddystoneFrame");
            if (value is ProximityBeaconFrame)
                return resourceLoader.GetString("ProximityBeaconFrame");

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
