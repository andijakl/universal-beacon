using System;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;
using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library
{
    internal class BLEScanCallback : ScanCallback
    {
        public event EventHandler<BeaconPacketArgs> OnAdvertisementPacketReceived;

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


                        var recordData = result.ScanRecord.GetBytes();
                        var rec = RecordParser.Parse(recordData);

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

            // result.Device;
        }
    }
}
