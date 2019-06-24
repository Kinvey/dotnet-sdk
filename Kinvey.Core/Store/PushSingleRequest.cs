using Kinvey;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey
{
    internal class PushSingleRequest<T> : WriteRequest<T, PushDataStoreResponse<T>>
    {
        int limit;
        int offset;

        PushDataStoreResponse<T> response;

        internal PushSingleRequest(AbstractClient client, string collection, ICache<T> cache, ISyncQueue queue, WritePolicy policy)
            : base(client, collection, cache, queue, policy)
        {
            limit = 10;
            offset = 0;

            response = new PushDataStoreResponse<T>();
        }

        public override async Task<PushDataStoreResponse<T>> ExecuteAsync()
        {
            List<PendingWriteAction> pendingActions = SyncQueue.GetFirstN(limit, offset);

            while (pendingActions != null && pendingActions.Count > 0)
            {
                var tasks = new List<Task<T>>();
                foreach (PendingWriteAction pwa in pendingActions)
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
                try
                {
                    await Task.WhenAll(tasks.ToArray());
                }
                catch (Exception e)
                {
                    //Do nothing for now
                    response.AddKinveyException(new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK,
                                                                    EnumErrorCode.ERROR_JSON_RESPONSE,
                                                                    "",
                                                                   e));  // TODO provide correct exception
                }

                List<T> resultEntities = new List<T>();
                int resultCount = 0;
                foreach (var t in tasks)
                {
                    if (!EqualityComparer<T>.Default.Equals(t.Result, default(T)))
                    {
                        resultEntities.Add(t.Result);
                    }

                    resultCount++;
                }

                response.AddEntities(resultEntities);
                response.PushCount += resultCount;

                pendingActions = SyncQueue.GetFirstN(limit, offset);
            }

            return response;
        }

        public override Task<bool> Cancel()
        {
            throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PushSingleRequest not implemented.");
        }

        private async Task<T> HandlePushPOST(PendingWriteAction pwa)
        {
            T entity = default(T);

            try
            {
                int result = 0;

                string tempID = pwa.entityId;

                entity = Cache.FindByID(pwa.entityId);

                JObject obj = JObject.FromObject(entity);
                obj["_id"] = null;
                entity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

                NetworkRequest<T> request = Client.NetworkFactory.buildCreateRequest<T>(pwa.collection, entity);
                entity = await request.ExecuteAsync();

                Cache.UpdateCacheSave(entity, tempID);

                result = SyncQueue.Remove(pwa);

                if (result == 0)
                {
                    offset++;
                }
            }
            catch (KinveyException ke)
            {
                response.AddKinveyException(ke);
                offset++;
            }
            return entity;
        }

        private async Task<T> HandlePushPUT(PendingWriteAction pwa)
        {
            T entity = default(T);

            try
            {
                int result = 0;

                string tempID = pwa.entityId;
                entity = Cache.FindByID(pwa.entityId);

                NetworkRequest<T> request = Client.NetworkFactory.buildUpdateRequest<T>(pwa.collection, entity, pwa.entityId);
                entity = await request.ExecuteAsync();

                Cache.UpdateCacheSave(entity, pwa.entityId);

                result = SyncQueue.Remove(pwa);

                if (result == 0)
                {
                    offset++;
                }
            }
            catch (KinveyException ke)
            {
                response.AddKinveyException(ke);
                offset++;
            }

            return entity;
        }

        private async Task<T> HandlePushDELETE(PendingWriteAction pwa)
        {
            try
            {
                int result = 0;

                NetworkRequest<KinveyDeleteResponse> request = Client.NetworkFactory.buildDeleteRequest<KinveyDeleteResponse>(pwa.collection, pwa.entityId);
                KinveyDeleteResponse kdr = await request.ExecuteAsync();

                if (kdr.count == 1)
                {
                    result = SyncQueue.Remove(pwa);

                    if (result == 0)
                    {
                        offset++;
                    }
                }
            }
            catch (KinveyException ke)
            {
                response.AddKinveyException(ke);
                offset++;
            }

            return default(T);
        }
    }
}
