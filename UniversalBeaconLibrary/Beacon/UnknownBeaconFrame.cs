// Copyright 2015 - 2017 Andreas Jakl. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// Based on the Google Eddystone specification, 
// available under Apache License, Version 2.0 from
// https://github.com/google/eddystone
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

namespace UniversalBeaconLibrary.Beacon
{
    /// <summary>
    /// Generic beacon frame class for a frame not known to this library.
    /// It provides access to the raw payload for other classes to analyze.
    /// </summary>
    public class UnknownBeaconFrame : BeaconFrameBase
    {
        public UnknownBeaconFrame(byte[] payload) : base(payload)
        {
        }
    }
}