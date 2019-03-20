// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using CoreBluetooth;
using Foundation;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library
{
    public class CocoaBluetoothPacketProvider : NSObject, IBluetoothPacketProvider
    {
        public event EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;
        public event EventHandler<BTError> WatcherStopped;

        private readonly CocoaBluetoothCentralDelegate _centralDelegate;
        private readonly CBCentralManager _central;

        public CocoaBluetoothPacketProvider()
        {
            Debug.WriteLine("BluetoothPacketProvider()");

            _centralDelegate = new CocoaBluetoothCentralDelegate();
            _central = new CBCentralManager(_centralDelegate, null);
        }

        private void ScanCallback_OnAdvertisementPacketReceived(object sender, BLEAdvertisementPacketArgs e)
        {
            AdvertisementPacketReceived?.Invoke(this, e);
        }

        public void Start()
        {
            Debug.WriteLine("BluetoothPacketProvider:Start()");
            _centralDelegate.OnAdvertisementPacketReceived += ScanCallback_OnAdvertisementPacketReceived;

            // Wait for the PoweredOn state

            //if(CBCentralManagerState.PoweredOn == central.State) {
            //    central.ScanForPeripherals(peripheralUuids: new CBUUID[] { },
            //                                               options: new PeripheralScanningOptions { AllowDuplicatesKey = false });
            //}
        }

        public void Stop()
        {
            Debug.WriteLine("BluetoothPacketProvider:Stop()");
            _centralDelegate.OnAdvertisementPacketReceived -= ScanCallback_OnAdvertisementPacketReceived;
 
            _central.StopScan();
            WatcherStopped?.Invoke(sender: this, e: new BTError(BTError.BluetoothError.Success));
        }
    }
}