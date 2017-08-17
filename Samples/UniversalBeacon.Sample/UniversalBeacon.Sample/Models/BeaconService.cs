using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using UniversalBeaconLibrary;
using Xamarin.Forms;

namespace EnRoute.Mobile.Models
{
    class BeaconService : IDisposable
    {
        private BeaconManager m_manager;

        public BeaconService()
        {
            // get the platform-specific provider
            var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();

            // create a beacon manager, giving it an invoker to marshal collection changes to the UI thread
            m_manager = new BeaconManager(provider, (a) =>
            {
                Device.BeginInvokeOnMainThread(a);
            });

            //m_manager = new BeaconManager(provider);

            m_manager.Start();
        }

        public void Dispose()
        {
            if (m_manager != null)
            {
                m_manager.Stop();
            }
        }

        public ObservableCollection<Beacon> Beacons
        {
            get { return m_manager.BluetoothBeacons; }
        }
    }
}
