// Copyright 2018 - 2026 Xavier Schott, Andreas Jakl. All rights reserved.
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
using CoreBluetooth;
using Foundation;

namespace UniversalBeacon.Library;

/// <summary>
/// Forwards the CoreBluetooth central manager callbacks to the packet provider.
/// </summary>
internal sealed class CocoaBluetoothCentralDelegate : CBCentralManagerDelegate
{
    public event EventHandler<CBManagerState>? StateUpdated;
    public event EventHandler<(CBPeripheral Peripheral, NSDictionary AdvertisementData, NSNumber Rssi)>? PeripheralDiscovered;

    public override void UpdatedState(CBCentralManager central)
    {
        StateUpdated?.Invoke(this, central.State);
    }

    public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral,
        NSDictionary advertisementData, NSNumber RSSI)
    {
        PeripheralDiscovered?.Invoke(this, (peripheral, advertisementData, RSSI));
    }
}
