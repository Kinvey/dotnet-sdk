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
			List<PendingWriteAction> pendingActions = SyncQueue.GetAll ();
			DataStoreResponse response = new DataStoreResponse ();
			foreach (PendingWriteAction pwa in pendingActions)
			{
				try
				{
					string tempID = pwa.entityId;
					T entity = Cache.FindByID(pwa.entityId);

					if (String.Equals("POST", pwa.action))
					{
						JObject obj = JObject.FromObject(entity);
						obj["_id"] = null;
						entity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

						NetworkRequest<T> request = Client.NetworkFactory.buildCreateRequest<T>(pwa.collection, entity);
						entity = await request.ExecuteAsync();

						Cache.UpdateCacheSave(entity, tempID);
					}
					else if (String.Equals("PUT", pwa.action))
					{
						NetworkRequest<T> request = Client.NetworkFactory.buildUpdateRequest<T>(pwa.collection, entity, pwa.entityId);
						await request.ExecuteAsync();
					}
					else if (String.Equals("DELETE", pwa.action))
					{
						NetworkRequest<KinveyDeleteResponse> request = Client.NetworkFactory.buildDeleteRequest<KinveyDeleteResponse>(pwa.collection, pwa.entityId);
						await request.ExecuteAsync();
					}

					SyncQueue.Remove(tempID);
					response.Count++;
				}
				catch (Exception e)
				{
					//Do nothing for now
					response.addError(new KinveyJsonError());	//TODO
				}
			}

			return response;
		}

		 public override Task<bool> Cancel() {
			throw new Exception ("not implemented");
		}

	}
}

