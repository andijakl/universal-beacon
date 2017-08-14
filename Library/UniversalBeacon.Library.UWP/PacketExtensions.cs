using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace UniversalBeaconLibrary
{
    internal static class PacketExtensions
    {
        public static BLEAdvertisementPacket ToUniversalBLEPacket(this BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var packet = new BLEAdvertisementPacket()
            {
                Timestamp = args.Timestamp,
                BluetoothAddress = args.BluetoothAddress,
                RawSignalStrengthInDBm = args.RawSignalStrengthInDBm,
                AdvertisementType = (BLEAdvertisementType)args.AdvertisementType
            };

            packet.Advertisement = args.Advertisement.ToUniversalAdvertisement();
            return packet;
        }

        public static BLEAdvertisement ToUniversalAdvertisement(this BluetoothLEAdvertisement a)
        {
            var result = new BLEAdvertisement()
            {
                LocalName = a.LocalName
            };

            result.ServiceUuids.AddRange(a.ServiceUuids);

            foreach (var d in a.DataSections)
            {
                var data = new BLEAdvertisementDataSection();
                data.DataType = d.DataType;
                data.Data = d.Data.ToArray();

                result.DataSections.Add(data);
            }

            foreach (var m in a.ManufacturerData)
            {
                var data = new BLEManufacturerData();
                data.CompanyId = m.CompanyId;
                data.Data = m.Data.ToArray();

                result.ManufacturerData.Add(data);
            }

            return result;
        }
    }
}
