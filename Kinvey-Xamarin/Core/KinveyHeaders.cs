// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
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
	/// <summary>
	/// Default Kinvey specific headers added to every request.
	/// </summary>
    public class KinveyHeaders : List<HttpHeader>
    {
		/// <summary>
		/// The version of the library.
		/// </summary>
		public static string VERSION = "1.6.12";

		/// <summary>
		/// The kinvey API version key.
		/// </summary>
        private static string kinveyApiVersionKey = "X-Kinvey-API-Version";
		/// <summary>
		/// The kinvey API version.
		/// </summary>
        private static string kinveyApiVersion = "3";

		/// <summary>
		/// The user agent key.
		/// </summary>
        private static string userAgentKey = "user-agent";
		/// <summary>
		/// The user agent.
		/// </summary>
		private string userAgent = "xamarin-kinvey-http/" + VERSION;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyHeaders"/> class.
		/// </summary>
        public KinveyHeaders() : base()
        {
			this.Add(new HttpHeader() {Name = userAgentKey, Value = new List<string>(){userAgent}});
			this.Add(new HttpHeader() {Name = kinveyApiVersionKey, Value = new List<string>(){kinveyApiVersion}});
        }

    }
}
