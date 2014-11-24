// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;


namespace KinveyXamarin
{
    [JsonObject(MemberSerialization.OptIn)]
    public class KinveyAuthResponse
    {

        public KinveyAuthResponse() { }

		private static KinveyAuthResponse Parse(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<KinveyAuthResponse>(response.Content);
        }

        [JsonProperty("_id")]
        public string UserId { get; set; }

        [JsonProperty("_kmd")]
        public KinveyUserMetadata UserMetadata { get; set; }

        public string AuthToken
        {
            get { return (UserMetadata != null ? UserMetadata.AuthToken : null); }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class KinveyUserMetadata 
        {
            [JsonProperty("lmt")]
            public string LastModifiedTime {get; set;}

            [JsonProperty("authtoken")]
            public string AuthToken {get; set; }
        }

    }
}
