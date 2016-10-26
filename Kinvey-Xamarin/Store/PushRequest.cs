// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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

namespace KinveyXamarin
{
	public class PushRequest <T> : WriteRequest<T, DataStoreResponse>
	{
		public PushRequest(AbstractClient client, string collection, ICache<T> cache, ISyncQueue queue, WritePolicy policy)
			: base (client, collection, cache, queue, policy)
		{
		}

		public override async Task <DataStoreResponse> ExecuteAsync()
		{
			DataStoreResponse response = new DataStoreResponse();

			int limit = 3;
			int offset = 0;

			List<PendingWriteAction> pendingActions = SyncQueue.GetFirstN(limit, offset);

			while (pendingActions != null && pendingActions.Count > 0)
			{
				var tasks = new List<Task<int>>();
				foreach (PendingWriteAction pwa in pendingActions)
				{
					try
					{
						if (String.Equals("POST", pwa.action))
						{
							tasks.Add(HandlePushPOST(pwa));
						}
						else if (String.Equals("PUT", pwa.action))
						{
							tasks.Add(HandlePushPUT(pwa));
						}
						else if (String.Equals("DELETE", pwa.action))
						{
							tasks.Add(HandlePushDELETE(pwa));
						}
					}
					catch (Exception e)
					{
						//Do nothing for now
						response.AddError(new KinveyJsonError());	//TODO
					}
				}

				await Task.WhenAll(tasks.ToArray());
				foreach (var t in tasks)
				{
					response.Count += t.Result;
				}

				pendingActions = SyncQueue.GetFirstN(limit, offset);
			}

			return response;
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PushRequest not implemented.");
		}

		private async Task<int> HandlePushPOST(PendingWriteAction pwa)
		{
			int result = 0;

			string tempID = pwa.entityId;
			T entity = Cache.FindByID(pwa.entityId);

			JObject obj = JObject.FromObject(entity);
			obj["_id"] = null;
			entity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

			NetworkRequest<T> request = Client.NetworkFactory.buildCreateRequest<T>(pwa.collection, entity);
			entity = await request.ExecuteAsync();

			Cache.UpdateCacheSave(entity, tempID);

			result = SyncQueue.Remove(tempID);

			return result;
		}

		private async Task<int> HandlePushPUT(PendingWriteAction pwa)
		{
			int result = 0;

			string tempID = pwa.entityId;
			T entity = Cache.FindByID(pwa.entityId);

			NetworkRequest<T> request = Client.NetworkFactory.buildUpdateRequest<T>(pwa.collection, entity, pwa.entityId);
			await request.ExecuteAsync();

			result = SyncQueue.Remove(tempID);

			return result;
		}

		private async Task<int> HandlePushDELETE(PendingWriteAction pwa)
		{
			int result = 0;

			NetworkRequest<KinveyDeleteResponse> request = Client.NetworkFactory.buildDeleteRequest<KinveyDeleteResponse>(pwa.collection, pwa.entityId);
			KinveyDeleteResponse kdr = await request.ExecuteAsync();

			if (kdr.count == 1)
			{
				result = SyncQueue.Remove(pwa.entityId);
			}

			return result;
		}
	}
}
