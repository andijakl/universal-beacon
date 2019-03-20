// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Sample.ViewCells;
using Xamarin.Forms;

namespace UniversalBeacon.Sample
{
    internal class FrameTemplateSelector : DataTemplateSelector
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
            if (item is Beacon beacon && beacon.BeaconType == Beacon.BeaconTypeEnum.Eddystone)
            {
                return _eddystoneTlmTemplate;
            }
            return _otherTemplate;
        }
    }
}
