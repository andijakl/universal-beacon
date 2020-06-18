using System;
using System.Diagnostics;
using System.Xml.XPath;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library
{
    internal class BLEScanCallback : ScanCallback
    {
        private readonly string LogTag = nameof(BLEScanCallback);

        public event EventHandler<BeaconPacketArgs> OnAdvertisementPacketReceived;

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            Debug.WriteLine($"{LogTag} scan failed, error: {errorCode}");

            base.OnScanFailed(errorCode);
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            switch (result.Device.Type)
            {
                case BluetoothDeviceType.Le:
                case BluetoothDeviceType.Unknown:
                    try
                    {
                        var p = new BeaconPacket
                        {
                            // address will be in the form "D1:36:E6:9D:46:52"
                            BluetoothAddress = result.Device.Address.ToNumericAddress(),    
                            RawSignalStrengthInDBm = (short) result.Rssi,
                            // TODO: probably needs adjustment
                            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(result.TimestampNanos / 1000),
                        };

                        Debug.WriteLine($"{LogTag} BLE advertisement received from {result.Device.Address}");
                        Debug.WriteLine($"{LogTag} BLE advertisement power {result.TxPower}");

                        // TODO: parse manufacturer data to get beaconing UUID, etc
                        // var manufacturerData = result.ScanRecord.ManufacturerSpecificData;

                        OnAdvertisementPacketReceived?.Invoke(this, new BeaconPacketArgs(p));
                    }
                    catch (Exception)
                    {
                        // TODO
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
