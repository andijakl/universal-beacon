using System;
using System.Collections.ObjectModel;
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

            // create a beacon manager, giving it an invoker to marshal collection changes to the UI thread
            _manager = new BeaconManager(provider, Device.BeginInvokeOnMainThread);

            //_manager = new BeaconManager(provider);

            _manager.Start();
        }

        public void Dispose()
        {
            _manager?.Stop();
        }

        public ObservableCollection<Beacon> Beacons => _manager.BluetoothBeacons;
    }
}
