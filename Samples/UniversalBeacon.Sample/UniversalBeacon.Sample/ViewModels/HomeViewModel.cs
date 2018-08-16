using OpenNETCF.IoC;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Sample.Models;

namespace UniversalBeacon.Sample.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BeaconService _service;
        public ObservableCollection<Beacon> Beacons => _service?.Beacons;
        private Beacon _selectedBeacon;
        
        public async Task RequestPermissions()
        {
            await RequestLocationPermission();
        }

        private async Task RequestLocationPermission()
        {
            // Actually coarse location would be enough, the plug-in only provides a way to request fine location
            var requestedPermissions = await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Location);
            var requestedPermissionStatus = requestedPermissions[Plugin.Permissions.Abstractions.Permission.Location];
            Debug.WriteLine("Location permission status: " + requestedPermissionStatus);
            if (requestedPermissionStatus == PermissionStatus.Granted)
            {
                Debug.WriteLine("Starting beacon service...");
                StartBeaconService();
            }
        }

        private void StartBeaconService()
        {
            _service = RootWorkItem.Services.Get<BeaconService>();
            if (_service == null)
            {
                _service = RootWorkItem.Services.AddNew<BeaconService>();
                if (_service.Beacons != null) _service.Beacons.CollectionChanged += Beacons_CollectionChanged;
            }
        }

        private void Beacons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine($"Beacons_CollectionChanged {sender} e {e}");
        }


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
