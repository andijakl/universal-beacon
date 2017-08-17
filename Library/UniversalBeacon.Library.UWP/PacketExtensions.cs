using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.UWP
{
    internal static class PacketExtensions
    {
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

        public static BLEAdvertisement ToUniversalAdvertisement(this BluetoothLEAdvertisement convertBleAdvertisment)
        {
            var result = new BLEAdvertisement
            {
                LocalName = convertBleAdvertisment.LocalName
            };

            result.ServiceUuids.AddRange(convertBleAdvertisment.ServiceUuids);

            foreach (var curDataSection in convertBleAdvertisment.DataSections)
            {
                var data = new BLEAdvertisementDataSection
                {
                    DataType = curDataSection.DataType,
                    Data = curDataSection.Data.ToArray()
                };

                result.DataSections.Add(data);
            }

            foreach (var curManufacturerData in convertBleAdvertisment.ManufacturerData)
            {
                var data = new BLEManufacturerData
                {
                    CompanyId = curManufacturerData.CompanyId,
                    Data = curManufacturerData.Data.ToArray()
                };

                result.ManufacturerData.Add(data);
            }

            return result;
        }
    }
}
