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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UniversalBeacon.Library.Core.Entities;

namespace WindowsBeacons
{
    public class FrameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnknownBeaconFrameTemplate { get; set; }

        public DataTemplate TlmEddystoneFrameTemplate { get; set; }

        public DataTemplate UidEddystoneFrameTemplate { get; set; }

        public DataTemplate UrlEddystoneFrameTemplate { get; set; }

        public DataTemplate EidEddystoneFrameTemplate { get; set; }

        public DataTemplate ProximityBeaconFrameTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var itemType = item.GetType();

            if (itemType == typeof(UnknownBeaconFrame))
                return UnknownBeaconFrameTemplate;
            if (itemType == typeof(TlmEddystoneFrame))
                return TlmEddystoneFrameTemplate;
            if (itemType == typeof(UidEddystoneFrame))
                return UidEddystoneFrameTemplate;
            if (itemType == typeof(UrlEddystoneFrame))
                return UrlEddystoneFrameTemplate;
            if (itemType == typeof(EidEddystoneFrame))
                return EidEddystoneFrameTemplate;
            if (itemType == typeof (ProximityBeaconFrame))
                return ProximityBeaconFrameTemplate;

            return base.SelectTemplateCore(item);

        }
    }
}
