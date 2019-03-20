// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
//    http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License. 

using System;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.Core.Interfaces
{    
    public interface IBluetoothPacketProvider
    {
        /// <summary>
        /// Event is invoked whenever a new Bluetooth LE Advertisment Packet has been received.
        /// Usually handled directly by the library. If required, your app implementation can
        /// also subscribe to get notified about events.
        /// </summary>
        event EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;
        /// <summary>
        /// Wrapper for the Bluetooth LE Watcher stopped event of the underlying OS.
        /// Currently used by UWP platform and invoked whenever there has been an issue
        /// starting the Bluetooth LE watcher or if an issue occured while watching for
        /// beacons (e.g., if the user turned off Bluetooth while the app is running).
        /// </summary>
        event EventHandler<BTError> WatcherStopped;
        /// <summary>
        /// Start watching for Bluetooth beacons.
        /// </summary>
        void Start();
        /// <summary>
        /// Stop watching for Bluetooth beacons.
        /// </summary>
        void Stop();
    }
}
