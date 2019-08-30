// Copyright (c) 2017, Kinvey, Inc. All rights reserved.
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
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// Subscribe request built for use by a <see cref="Kinvey.DataStore{T}"/>
	/// </summary>
	public class SubscribeRequest<T> : Request<T, Newtonsoft.Json.Linq.JObject>
	{
		private List<string> EntityIDs { get; }
		private KinveyDelegate<List<T>> cacheDelegate;

		string Collection { get; set; }
		string DeviceID { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Kinvey.SubscribeRequest`1"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="collection">Collection.</param>
		public SubscribeRequest(AbstractClient client, string collection, string deviceID)
			: base(client)
		{
			Collection = collection;

			DeviceID = deviceID;
		}

		public override async Task<JObject> ExecuteAsync()
		{
			JObject result = default(JObject);

			result = await Client.NetworkFactory.BuildSubscribeRequest<JObject>(Collection, DeviceID).ExecuteAsync();

			return result;
		}

		public override async Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on FindRequest not implemented.");
		}
	}
}
