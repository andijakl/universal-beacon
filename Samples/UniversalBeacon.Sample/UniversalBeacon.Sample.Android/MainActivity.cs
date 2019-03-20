// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using Android.App;
using Android.Content.PM;
using Android.OS;
using OpenNETCF.IoC;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using UniversalBeacon.Library;
using UniversalBeacon.Library.Core.Interfaces;

namespace UniversalBeacon.Sample.Droid
{
    [Activity(Label = "Universal Beacon", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Required for the Request Permissions Plug-In
            CrossCurrentActivity.Current.Init(this, bundle);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
            

            var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();
            if (provider == null)
            {
                provider = new AndroidBluetoothPacketProvider(this);
                RootWorkItem.Services.Add<IBluetoothPacketProvider>(provider);
            }
            
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
    }

}
