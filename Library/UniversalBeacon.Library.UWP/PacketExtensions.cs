// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.UWP
{
    internal static class PacketExtensions
    {
        /// <summary>
        /// Convert the Windows specific Bluetooth LE advertisment data to the universal cross-platform structure
        /// used by the Universal Beacon Library.
        /// </summary>
        /// <param name="args">Windows Bluetooth LE advertisment data to convert to cross-platform format.</param>
        /// <returns>Packet data converted to cross-platform format.</returns>
        public static BLEAdvertisementPacket ToUniversalBLEPacket(this BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var packet = new BLEAdvertisementPacket
            {
                Timestamp = args.Timestamp,
                BluetoothAddress = args.BluetoothAddress,
                RawSignalStrengthInDBm = args.RawSignalStrengthInDBm,
                AdvertisementType = (BLEAdvertisementType) args.AdvertisementType,
                Advertisement = args.Advertisement.ToUniversalAdvertisement()
            };

            return packet;
        }

        /// <summary>
        /// Convert the Windows specific Bluetooth LE advertisement to the universal cross-platform structure
        /// used by the Universal Beacon Library.
        /// </summary>
        /// <param name="convertBleAdvertisement">Windows Bluetooth LE advertisement to convert to cross-platform format.</param>
        /// <returns>Packet converted to cross-platform format.</returns>
        public static BLEAdvertisement ToUniversalAdvertisement(this BluetoothLEAdvertisement convertBleAdvertisement)
        {
            if (convertBleAdvertisement == null) return null;

            var result = new BLEAdvertisement
            {
                LocalName = convertBleAdvertisement.LocalName
            };

            result.ServiceUuids.AddRange(convertBleAdvertisement.ServiceUuids);

            if (convertBleAdvertisement.DataSections != null)
            {
                foreach (var curDataSection in convertBleAdvertisement.DataSections)
                {
                    var data = new BLEAdvertisementDataSection
                    {
                        DataType = curDataSection.DataType,
                        Data = curDataSection.Data.ToArray()
                    };

                    result.DataSections.Add(data);
                }
            }

            if (convertBleAdvertisement.ManufacturerData != null)
            {
                foreach (var curManufacturerData in convertBleAdvertisement.ManufacturerData)
                {
                    var data = new BLEManufacturerData
                    {
                        CompanyId = curManufacturerData.CompanyId,
                        Data = curManufacturerData.Data.ToArray()
                    };

                    result.ManufacturerData.Add(data);
                }
            }

            return result;
        }
    }
}
