// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;

namespace UniversalBeacon.Library.Core.Interop
{    

    public class BLEAdvertisementPacketArgs : EventArgs
    {
        public BLEAdvertisementPacket Data { get; private set; }

        public BLEAdvertisementPacketArgs(BLEAdvertisementPacket data)
        {
            Data = data;
        }
    }
}
