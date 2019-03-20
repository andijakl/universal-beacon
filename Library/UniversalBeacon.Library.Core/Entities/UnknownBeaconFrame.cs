// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
//    http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License. 

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