using Foundation;
using OpenNETCF.IoC;
using UIKit;
using UniversalBeacon.Library;
using UniversalBeacon.Library.Core.Interfaces;

namespace UniversalBeacon.Sample.iOS
{
    [Register("AppDelegate")]

    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();
            if (provider == null)
            {
                provider = new iOSBluetoothPacketProvider();
                RootWorkItem.Services.Add<IBluetoothPacketProvider>(provider);
            }

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
