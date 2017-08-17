using OpenNETCF.IoC;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Sample.Models;

namespace UniversalBeacon.Sample.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly BeaconService _service;
        private Beacon _selectedBeacon;

        public HomeViewModel()
        {
            _service = RootWorkItem.Services.Get<BeaconService>();
            if (_service == null)
            {
                _service = RootWorkItem.Services.AddNew<BeaconService>();
                _service.Beacons.CollectionChanged += Beacons_CollectionChanged;
            }
        }

        private void Beacons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        public ObservableCollection<Beacon> Beacons => _service?.Beacons;

        public Beacon SelectedBeacon
        {
            get => _selectedBeacon;
            set
            {
                _selectedBeacon = value;
                PropertyChanged.Fire(this, "SelectedBeacon");
            }
        }
    }
}
