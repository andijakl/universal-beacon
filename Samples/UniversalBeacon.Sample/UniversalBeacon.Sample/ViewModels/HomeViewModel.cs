using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBeaconLibrary;

namespace UniversalBeacon.Sample.ViewModels
{
    public class HomeViewModel
    {
        private BeaconManager m_manager;

        public HomeViewModel()
        {
            // get the platform-specific provider
            var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();

            // create a beacon manager
            m_manager = new BeaconManager(provider);
        }
    }
}
