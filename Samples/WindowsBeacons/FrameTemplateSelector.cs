// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

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
