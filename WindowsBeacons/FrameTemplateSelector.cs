using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UniversalBeaconLibrary.Beacon;

namespace WindowsBeacons
{
    public class FrameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnknownBeaconFrameTemplate { get; set; }

        public DataTemplate TlmEddystoneFrameTemplate { get; set; }

        public DataTemplate UidEddystoneFrameTemplate { get; set; }

        public DataTemplate UrlEddystoneFrameTemplate { get; set; }

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

            return base.SelectTemplateCore(item);

        }
    }
}
