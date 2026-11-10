// Copyright 2015 - 2026 Andreas Jakl, Chris Tacke. All rights reserved.
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

using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library;

/// <summary>
/// Conversion of the Windows Bluetooth LE advertisement types to the platform-independent types.
/// </summary>
public static class WindowsPacketExtensions
{
    public static BLEAdvertisementPacket ToUniversalBLEPacket(this BluetoothLEAdvertisementReceivedEventArgs args)
    {
        return new BLEAdvertisementPacket
        {
            Timestamp = args.Timestamp,
            BluetoothAddress = args.BluetoothAddress,
            RawSignalStrengthInDBm = args.RawSignalStrengthInDBm,
            AdvertisementType = (BLEAdvertisementType)args.AdvertisementType,
            Advertisement = args.Advertisement.ToUniversalAdvertisement()
        };
    }

    public static BLEAdvertisement? ToUniversalAdvertisement(this BluetoothLEAdvertisement? advertisement)
    {
        if (advertisement == null) return null;

        var result = new BLEAdvertisement
        {
            LocalName = advertisement.LocalName
        };
        result.ServiceUuids.AddRange(advertisement.ServiceUuids);

        if (advertisement.DataSections != null)
        {
            foreach (var dataSection in advertisement.DataSections)
            {
                result.DataSections.Add(new BLEAdvertisementDataSection
                {
                    DataType = dataSection.DataType,
                    Data = dataSection.Data.ToArray()
                });
            }
        }

        if (advertisement.ManufacturerData != null)
        {
            foreach (var manufacturerData in advertisement.ManufacturerData)
            {
                result.ManufacturerData.Add(new BLEManufacturerData
                {
                    CompanyId = manufacturerData.CompanyId,
                    Data = manufacturerData.Data.ToArray()
                });
            }
        }

        return result;
    }
}
