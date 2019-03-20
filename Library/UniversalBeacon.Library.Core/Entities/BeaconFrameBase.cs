// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UniversalBeacon.Library.Core.Entities
{
    /// <summary>
    /// Abstract class for every Bluetooth Beacon frame.
    /// Common is that every frame has a payload / data, which derived classes can
    /// further parse and make it more accessible through custom properties depending
    /// on the beacon specification.
    /// </summary>
    public abstract class BeaconFrameBase : INotifyPropertyChanged
    {
        protected byte[] _payload;
        
        /// <summary>
        /// The raw byte payload of this beacon frame.
        /// Derived classes can add additional functionality to parse or update the payload.
        /// 
        /// Note: directly setting the payload does not lead to the class re-parsing the payload
        /// into its instance variables (if applicable in the derived class).
        /// Call ParsePayload() manually from the derived class if you wish to enable this behavior.
        /// </summary>
        public byte[] Payload
        {
            get => _payload;
            set
            {
                if (value == null)
                {
                    _payload = null;
                    return;
                }
                if (_payload != null && _payload.SequenceEqual(value)) return;
                _payload = new byte[value.Length];
                Array.Copy(value, _payload, value.Length);
                OnPropertyChanged();
            }
        }

        protected BeaconFrameBase()
        {
            
        }

        protected BeaconFrameBase(byte[] payload)
        {
            Payload = payload;
        }

        protected BeaconFrameBase(BeaconFrameBase other)
        {
            Payload = other.Payload;
        }

        /// <summary>
        /// Update payload based on supplied other frame.
        /// </summary>
        /// <param name="otherFrame"></param>
        public virtual void Update(BeaconFrameBase otherFrame)
        {
            Payload = otherFrame.Payload;
        }

        /// <summary>
        /// Checks if the payload is valid.
        /// Base implementation only checks if payload is not null.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return Payload != null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
