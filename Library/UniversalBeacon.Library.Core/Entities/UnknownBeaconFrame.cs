// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

namespace UniversalBeacon.Library.Core.Entities
{
    /// <summary>
    /// Generic beacon frame class for a frame not known to this library.
    /// It provides access to the raw payload for other classes to analyze.
    /// </summary>
    public class UnknownBeaconFrame : BeaconFrameBase
    {
        /// <summary>
        /// Construct a new instance of an unknown beacon frame.
        /// Only stores the payload, does not do any further processing.
        /// </summary>
        /// <param name="payload">Payload to parse for this frame type.</param>
        public UnknownBeaconFrame(byte[] payload) : base(payload)
        {
        }
    }
}