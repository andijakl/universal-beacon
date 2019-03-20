// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

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

        //private ObservableCollection<Beacon> _beacons;
        public ObservableCollection<Beacon> Beacons => _service?.Beacons;
        //{
        //    get => _beacons;
        //    set
        //    {
        //        _beacons = value;
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Beacons"));
        //    }
        //} // => _service?.Beacons;

        private Beacon _selectedBeacon;
        
        public async Task RequestPermissions()
        {
            await RequestLocationPermission();
        }

        private async Task RequestLocationPermission()
        {
            // Actually coarse location would be enough, the plug-in only provides a way to request fine location
            var requestedPermissions = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
            var requestedPermissionStatus = requestedPermissions[Permission.Location];
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
                if (_service.Beacons != null)
                {
                    _service.Beacons.CollectionChanged += Beacons_CollectionChanged;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Beacons"));
                    Debug.WriteLine("Beacon Service Initialized");
                }
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
