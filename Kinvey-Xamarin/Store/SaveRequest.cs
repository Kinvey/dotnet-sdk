using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	public class SaveRequest <T> : WriteRequest<T, T>
	{
		private T entity;

		public SaveRequest (T entity, AbstractClient client, string collection, ICache<T> cache, ISyncQueue sync, WritePolicy policy)
			: base (client, collection, cache, sync, policy)
		{
			this.entity = entity;
		}

		public override async Task<T> ExecuteAsync()
		{
			T savedEntity = default(T);
			NetworkRequest<T> request = null;

			JToken idToken = JObject.FromObject (entity) ["_id"];
			if (idToken != null &&
			    !String.IsNullOrEmpty(idToken.ToString()))
			{
				string entityID = idToken.ToString();
				request = Client.NetworkFactory.buildUpdateRequest(Collection, entity, entityID);
			}
			else
			{
				request = Client.NetworkFactory.buildCreateRequest(Collection, entity);
			}

			switch (Policy)
			{
				case WritePolicy.FORCE_LOCAL:
					// sync
					PendingWriteAction pendingAction = PendingWriteAction.buildFromRequest(request);

					string sm = request.RequestMethod;
					string tID = null;

					if (String.Equals("POST", sm))
					{
						tID = PrepareCacheSave(ref entity);
						savedEntity = Cache.Save(entity);
						pendingAction.entityId = tID;
					}
					else
					{
						savedEntity = Cache.Update(entity);
					}

					SyncQueue.Enqueue(pendingAction);

					break;

				case WritePolicy.FORCE_NETWORK:
					// network
					savedEntity = await request.ExecuteAsync ();
					break;

				case WritePolicy.NETWORK_THEN_LOCAL:
					// cache
					string saveMode = request.RequestMethod;
					string tempID = null;

					if (String.Equals("POST", saveMode))
					{
						tempID = PrepareCacheSave(ref entity);
						Cache.Save(entity);
					}
					else
					{
						Cache.Update(entity);
					}


					// network save
					savedEntity = await request.ExecuteAsync();

					if (tempID != null)
					{
						Cache.UpdateCacheSave(savedEntity, tempID);
					}

					break;

				default:
					throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_GENERAL, "Invalid write policy");
			}

			return savedEntity;
			//T saved = await this.Cache.SaveAsync (entity);
			//int result = await this.SyncQueue.Enqueue (PendingWriteAction.buildFromRequest <T> (request);
			//PendingWriteAction action = await this.SyncQueue.Pop ();
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on SaveRequest not implemented.");
		}

		private string PrepareCacheSave(ref T entity)
		{
			string guid = System.Guid.NewGuid().ToString();
			string tempID = "temp_" + guid;

			JObject obj = JObject.FromObject(entity);
			obj["_id"] = tempID;
			entity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

			return tempID;
		}
	}
}

