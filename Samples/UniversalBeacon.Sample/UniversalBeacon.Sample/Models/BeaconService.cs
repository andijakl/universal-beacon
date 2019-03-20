// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using OpenNETCF.IoC;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using Xamarin.Forms;

namespace UniversalBeacon.Sample.Models
{
    internal class BeaconService : IDisposable
    {
        private readonly BeaconManager _manager;

        public BeaconService()
        {
            // get the platform-specific provider
            var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();

            if(null != provider) {
                // create a beacon manager, giving it an invoker to marshal collection changes to the UI thread
                _manager = new BeaconManager(provider, Device.BeginInvokeOnMainThread);
                _manager.Start();
#if DEBUG
                provider.AdvertisementPacketReceived += Provider_AdvertisementPacketReceived;
#endif // DEBUG
            }
        }

        public void Dispose()
        {
#if DEBUG
            _manager.BeaconAdded -= Manager_BeaconAdded;
#endif
            _manager?.Stop();
        }

        public ObservableCollection<Beacon> Beacons => _manager?.BluetoothBeacons;

#if DEBUG
        private void Manager_BeaconAdded(object sender, Beacon e)
        {
            Debug.WriteLine($"Manager_BeaconAdded {sender} Beacon {e}");
        }

        private void Provider_AdvertisementPacketReceived(object sender, UniversalBeacon.Library.Core.Interop.BLEAdvertisementPacketArgs e)
        {
            Debug.WriteLine($"Provider_AdvertisementPacketReceived {sender} Beacon {e}");
        }
#endif // DEBUG
    }
}
