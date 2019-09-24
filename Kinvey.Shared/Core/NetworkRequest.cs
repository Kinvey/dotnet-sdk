// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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
namespace Kinvey
{
    /// <summary>
    /// The class for creating network requests.
    /// </summary>
    /// <typeparam name="T">The object type of the network request and response.</typeparam>
    public class NetworkRequest <T> : AbstractKinveyClientRequest <T>
	{
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="requestMethod">Request method.</param>
        /// <param name="uriTemplate">URI template.</param>
        /// <param name="httpContent">Http content.</param>
        /// <param name="uriParameters">URI parameters.</param>
		public NetworkRequest (AbstractClient client, string requestMethod, string uriTemplate, Object httpContent, Dictionary<string, string> uriParameters) :
		base (client, client.BaseUrl, requestMethod, uriTemplate, httpContent, uriParameters)
		{}

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="baseURL">Base URL.</param>
        /// <param name="requestMethod">Request method.</param>
        /// <param name="uriTemplate">URI template.</param>
        /// <param name="httpContent">Http content.</param>
        /// <param name="uriParameters">URI parameters.</param>
        public NetworkRequest(AbstractClient client, string baseURL, string requestMethod, string uriTemplate, Object httpContent, Dictionary<string, string> uriParameters):
		base (client, baseURL, requestMethod, uriTemplate, httpContent, uriParameters)
		{}

	}
}
