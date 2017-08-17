using System;
using System.Collections.Generic;
using System.Text;
using UniversalBeacon.Library.Core.Constants;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Sample.ViewCells;
using Xamarin.Forms;

namespace UniversalBeacon.Sample
{
    class FrameTemplateSelector : Xamarin.Forms.DataTemplateSelector
    {
        private readonly DataTemplate _eddystoneTlmTemplate;
        private readonly DataTemplate _otherTemplate;

        public FrameTemplateSelector()
        {
            _eddystoneTlmTemplate = new DataTemplate(typeof(EddystoneTLMViewCell));
            _otherTemplate = new DataTemplate(typeof(GenericViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Beacon beacon && beacon.BeaconType == BeaconType.Eddystone)
            {
                return _eddystoneTlmTemplate;
            }
            return _otherTemplate;
        }
    }
}
