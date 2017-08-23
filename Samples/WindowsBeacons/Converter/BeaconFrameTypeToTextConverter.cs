// Copyright 2015 - 2017 Andreas Jakl. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.

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
