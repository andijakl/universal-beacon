using System;
using System.Collections.Generic;
using System.Text;
using UniversalBeacon.Sample.ViewCells;
using UniversalBeaconLibrary;
using Xamarin.Forms;

namespace UniversalBeacon.Sample
{
    class FrameTemplateSelector : Xamarin.Forms.DataTemplateSelector
    {
        private readonly DataTemplate EddystoneTLMTemplate;
        private readonly DataTemplate OtherTemplate;

        public FrameTemplateSelector()
        {
            EddystoneTLMTemplate = new DataTemplate(typeof(EddystoneTLMViewCell));
            OtherTemplate = new DataTemplate(typeof(GenericViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var beacon = item as Beacon;
            if (beacon.BeaconType == Beacon.BeaconTypeEnum.Eddystone)
            {
                return EddystoneTLMTemplate;
            }
            else
            {
                return OtherTemplate;
            }

        }
    }
}
