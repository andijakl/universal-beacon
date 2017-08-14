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
    public class AndroidBluetoothPacketProvider : Java.Lang.Object, IBluetoothPacketProvider
    {
        
        public event EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;

        private Context m_context;
        private BluetoothManager m_manager;
        private BluetoothAdapter m_adapter;
        private BLEScanCallback m_scanCallback;

        public AndroidBluetoothPacketProvider(Context context)
        {
            m_context = context;
            m_manager = (BluetoothManager)m_context.GetSystemService("bluetooth");
            m_adapter = m_manager.Adapter;
            m_scanCallback = new BLEScanCallback();
            m_scanCallback.OnAdvertisementPacketReceived += ScanCallback_OnAdvertisementPacketReceived;
        }

        private void ScanCallback_OnAdvertisementPacketReceived(object sender, BLEAdvertisementPacketArgs e)
        {
            AdvertisementPacketReceived?.Invoke(this, e);
        }

        public void Start()
        {
            try
            {
                m_adapter.BluetoothLeScanner.StartScan(m_scanCallback);
            }
            catch (Exception ex)
            {
            }
        }

        public void Stop()
        {
            m_adapter.CancelDiscovery();
        }
    }
}
