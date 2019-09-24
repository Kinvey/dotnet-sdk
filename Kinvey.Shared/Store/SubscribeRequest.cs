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

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
    /// <summary>
    /// Subscribe request built for use by a <see cref="DataStore{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the network request.</typeparam>
    public class SubscribeRequest<T> : Request<T, Newtonsoft.Json.Linq.JObject>
	{
		string Collection { get; set; }
		string DeviceID { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribeRequest{T}"/> class.
        /// </summary>
        /// <param name="client">Client that the user is logged in.</param>
        /// <param name="collection">Collection name.</param>
        /// <param name="deviceID">Device Id.</param>
        public SubscribeRequest(AbstractClient client, string collection, string deviceID)
			: base(client)
		{
			Collection = collection;

			DeviceID = deviceID;
		}


        /// <summary>
        /// Executes the request asynchronously.
        /// </summary>
        /// <returns> The async task with the request result.</returns>
        public override async Task<JObject> ExecuteAsync()
		{
			JObject result = default(JObject);

			result = await Client.NetworkFactory.BuildSubscribeRequest<JObject>(Collection, DeviceID).ExecuteAsync();

			return result;
		}

        /// <summary>
        /// Communicates the request for cancellation.
        /// </summary>
        /// <returns>The async task with the boolean result. If the result is <c>true</c> then the request was canceled, otherwise <c>false</c>.</returns>
        public override async Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on FindRequest not implemented.");
		}
	}
}
