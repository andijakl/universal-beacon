using Android.App;
using Android.Content.PM;
using Android.OS;
using OpenNETCF.IoC;
using UniversalBeacon.Library;
using UniversalBeacon.Library.Core.Interfaces;

namespace UniversalBeacon.Sample.Droid
{
    [Activity(Label = "UniversalBeacon.Sample", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();
            if (provider == null)
            {
                provider = new AndroidBluetoothPacketProvider(this);
                RootWorkItem.Services.Add<IBluetoothPacketProvider>(provider);
            }

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}

