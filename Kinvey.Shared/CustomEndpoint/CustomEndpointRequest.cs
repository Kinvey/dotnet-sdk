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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kinvey
{
    /// <summary>
    /// Executes a custom endpoint expecting a single result.
    /// </summary>
    /// <typeparam name="I">The type of request.</typeparam>
    /// <typeparam name="O">The type of response.</typeparam>
    public class CustomEndpointRequest<I, O> : AbstractKinveyClientRequest<O>
	{
		private const string REST_PATH = "rpc/{appKey}/custom/{endpoint}";

        /// <summary>
        /// The endpoint.
        /// </summary>
        /// <value>The string value with endpoint.</value>
        [JsonProperty]
		public string endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEndpointRequest{I, O}"/> class.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="input">Input.</param>
        /// <param name="urlProperties">URL properties.</param>
        public CustomEndpointRequest(AbstractClient client, string endpoint, I input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties)
		{
			this.endpoint = endpoint;
		}
	}
}
