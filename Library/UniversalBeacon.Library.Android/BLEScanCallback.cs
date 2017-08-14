using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Runtime;

namespace UniversalBeaconLibrary
{
    internal class BLEScanCallback : ScanCallback
    {
        public event EventHandler<BLEAdvertisementPacketArgs> OnAdvertisementPacketReceived;

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            switch (result.Device.Type)
            {
                case BluetoothDeviceType.Le:
                    var p = new BLEAdvertisementPacket();

                    // address will be in the form "D1:36:E6:9D:46:52"
                    p.BluetoothAddress = result.Device.Address.ToNumericAddress();
                    p.RawSignalStrengthInDBm = (short)result.Rssi;
                    p.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(result.TimestampNanos / 1000); // TODO: probably needs adjustment
                    p.AdvertisementType = (BLEAdvertisementType)result.ScanRecord.AdvertiseFlags; // TODO: validate this
                    p.Advertisement = new BLEAdvertisement()
                    {
                        LocalName = result.ScanRecord.DeviceName
                    };

                    var recordData = result.ScanRecord.GetBytes();
                    var rec = RecordParser.Parse(recordData);

                    if (result.ScanRecord.ServiceUuids != null)
                    {
                        foreach(var svc in result.ScanRecord.ServiceUuids)
                        {
                            var guid = new Guid(svc.Uuid.ToString());
                            var data = result.ScanRecord.GetServiceData(svc);

                            p.Advertisement.ServiceUuids.Add(guid);
                            p.Advertisement.DataSections.Add(new BLEAdvertisementDataSection()
                            {
                                DataType = 0, // TODO: where does this come from?
                                Data = data
                            });
                        }
                    }

                    foreach (var o in rec)
                    {
                        var md = o as BLEManufacturerData;
                        if(md != null)
                        {
                            p.Advertisement.ManufacturerData.Add(md);
                        }
                    }

                    OnAdvertisementPacketReceived?.Invoke(this, new BLEAdvertisementPacketArgs(p));
                    break;
                default:
                    break;
            }

            // result.Device;
        }
    }
}
