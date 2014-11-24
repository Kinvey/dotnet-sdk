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
using RestSharp;

namespace KinveyXamarin
{
    public class KinveyHeaders : List<HttpHeader>
    {
        private static string version = "2.6.6";

        private static string kinveyApiVersionKey = "X-Kinvey-API-Version";
        private static string kinveyApiVersion = "3";

        private static string userAgentKey = "user-agent";
        private string userAgent = "dotnet-kinvey-http/" + version;

        public KinveyHeaders() : base()
        {
			this.Add(new HttpHeader() {Name = userAgentKey, Value = new List<string>(){userAgent}});
			this.Add(new HttpHeader() {Name = kinveyApiVersionKey, Value = new List<string>(){kinveyApiVersion}});
        }

    }
}
