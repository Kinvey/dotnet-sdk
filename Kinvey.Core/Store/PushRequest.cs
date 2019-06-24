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
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	public class PushRequest <T> : WriteRequest<T, PushDataStoreResponse<T>>
	{
		public PushRequest(AbstractClient client, string collection, ICache<T> cache, ISyncQueue queue, WritePolicy policy)
			: base (client, collection, cache, queue, policy)
		{

		}

		public override async Task <PushDataStoreResponse<T>> ExecuteAsync()
		{
            var response = new PushDataStoreResponse<T>();

            if (HelperMethods.IsLessThan(Client.ApiVersion, 5))
            {
                var pushSingleRequest = new PushSingleRequest<T>(Client, Collection, Cache, SyncQueue, Policy);
                response = await pushSingleRequest.ExecuteAsync();
            }
            else
            {
                var pushMultiRequest = new PushMultiRequest<T>(Client, Collection, Cache, SyncQueue, Policy);
                response = await pushMultiRequest.ExecuteAsync();
            }

            return response;
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PushRequest not implemented.");
		}		
	}
}
