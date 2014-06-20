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


namespace Kinvey.DotNet.Framework.Core
{
	public class KinveyAuthenticator : IAuthenticator
    {
        private static readonly string AuthHeaderFormat = "Kinvey {0}";
        private readonly string authToken;

		public KinveyAuthenticator(string authToken) {
			this.authToken = authToken;
		}

		public void Authenticate(IRestClient client, IRestRequest request) {

			if (!request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
			{
				
				var authHeader = string.Format(AuthHeaderFormat, authToken);
				request.AddParameter("Authorization", authHeader, ParameterType.HttpHeader);
			}
		}

		public void Authenticate(IRestRequest request) {

			if (!request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
			{

				var authHeader = string.Format(AuthHeaderFormat, authToken);
				request.AddParameter("Authorization", authHeader, ParameterType.HttpHeader);
			}
		}


    }
}
