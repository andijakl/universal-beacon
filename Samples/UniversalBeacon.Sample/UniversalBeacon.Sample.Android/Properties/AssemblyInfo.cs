using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Android.App;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Universal Beacon Library - Xamarin Android Sample")]
[assembly: AssemblyDescription("View information from nearby Bluetooth Beacons - currently supporting Eddystone and iBeacon.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Chris Tacke, Andreas Jakl")]
[assembly: AssemblyProduct("UniversalBeacon.Sample.Android")]
[assembly: AssemblyCopyright("Copyright © 2015 - 2018 Chris Tacke, Andreas Jakl")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("4.0.0.0")]
[assembly: AssemblyFileVersion("4.0.0.0")]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
