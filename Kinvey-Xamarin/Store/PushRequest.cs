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
			var tasks = new List<Task<int>>();

			List<PendingWriteAction> pendingActions = SyncQueue.GetAll();

			DataStoreResponse response = new DataStoreResponse();

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

					//response.Count++;
				}
				catch (Exception e)
				{
					//Do nothing for now
					response.addError(new KinveyJsonError());	//TODO
				}
			}

			Task.WaitAll(tasks.ToArray());
			foreach (var t in tasks)
			{
				response.Count += t.Result;
			}

			return response;
		}

		 public override Task<bool> Cancel() {
			throw new Exception ("not implemented");
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

