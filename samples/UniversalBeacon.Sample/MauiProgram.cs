using Microsoft.Extensions.Logging;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Sample.ViewModels;

namespace UniversalBeacon.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Platform-specific Bluetooth LE advertisement provider from the Universal Beacon Library.
#if ANDROID
        builder.Services.AddSingleton<IBluetoothPacketProvider>(
            _ => new UniversalBeacon.Library.AndroidBluetoothPacketProvider(Android.App.Application.Context));
#elif IOS || MACCATALYST
        builder.Services.AddSingleton<IBluetoothPacketProvider, UniversalBeacon.Library.CocoaBluetoothPacketProvider>();
#elif WINDOWS
        builder.Services.AddSingleton<IBluetoothPacketProvider, UniversalBeacon.Library.WindowsBluetoothPacketProvider>();
#endif

        // The beacon manager clusters the received advertisements into beacons.
        // Received packets are processed on the UI thread, as the beacon list is bound to the UI.
        builder.Services.AddSingleton(services => new BeaconManager(
            services.GetRequiredService<IBluetoothPacketProvider>(),
            MainThread.BeginInvokeOnMainThread));

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
