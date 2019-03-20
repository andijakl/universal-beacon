// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;

namespace UniversalBeacon.Library.Core.Entities
{
    /// <summary>
    /// Exception occured when parsing or assembling Bluetooth Beacons.
    /// </summary>
    public class BeaconException : Exception
    {
        public BeaconException() { }
        public BeaconException(string message) : base(message) { }
        public BeaconException(string message, Exception inner) : base(message, inner) { }
    }
}
