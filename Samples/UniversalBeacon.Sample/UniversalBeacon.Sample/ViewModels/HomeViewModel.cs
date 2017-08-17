using EnRoute.Mobile.Models;
using OpenNETCF.IoC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBeaconLibrary;
using Xamarin.Forms;

namespace UniversalBeacon.Sample.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BeaconService m_service;
        private Beacon m_selectedBeacon;

        public HomeViewModel()
        {
            m_service = RootWorkItem.Services.Get<BeaconService>();
            if (m_service == null)
            {
                m_service = RootWorkItem.Services.AddNew<BeaconService>();
                m_service.Beacons.CollectionChanged += Beacons_CollectionChanged;
            }
        }

        private void Beacons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        public ObservableCollection<Beacon> Beacons
        {
            get
            {
                return m_service?.Beacons;
            }
        }

        public Beacon SelectedBeacon
        {
            get { return m_selectedBeacon; }
            set
            {
                m_selectedBeacon = value;
                PropertyChanged.Fire(this, "SelectedBeacon");
            }
        }
    }
}
