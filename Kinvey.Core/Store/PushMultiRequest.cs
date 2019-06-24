using Kinvey;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey
{
    internal class PushMultiRequest<T> : WriteRequest<T, PushDataStoreResponse<T>>
    {
        PushDataStoreResponse<T> response;

        internal PushMultiRequest(AbstractClient client, string collection, ICache<T> cache, ISyncQueue queue, WritePolicy policy)
            : base(client, collection, cache, queue, policy)
        {           
            response = new PushDataStoreResponse<T>();
        }

        public override Task<bool> Cancel()
        {
            throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PushMultiRequest not implemented.");
        }

        public override async Task<PushDataStoreResponse<T>> ExecuteAsync()
        {
            var offset = 0;
            var limit = 10 * Constants.NUMBER_LIMIT_OF_ENTITIES;
            var pendingPostActions = SyncQueue.GetFirstN(limit, offset, "POST");

            var tasks = new List<Task<KinveyMultiInsertResponse<T>>>();

            while (pendingPostActions != null && pendingPostActions.Count > 0)
            {
                var realCountOfMultiInsertOperations = pendingPostActions.Count / (double)Constants.NUMBER_LIMIT_OF_ENTITIES;
               realCountOfMultiInsertOperations = Math.Ceiling(realCountOfMultiInsertOperations);

                for (var index = 0; index < realCountOfMultiInsertOperations; index++)
                {
                    var pendingWritePostActionsForPush = pendingPostActions.Skip(index * Constants.NUMBER_LIMIT_OF_ENTITIES).Take(Constants.NUMBER_LIMIT_OF_ENTITIES).ToList();

                    if (pendingWritePostActionsForPush.Count > 0)
                    {
                        tasks.Add(HandlePushMultiPOST(pendingWritePostActionsForPush));
                        await Task.Delay(100000);
                    }
                }

                offset += limit;
                pendingPostActions = SyncQueue.GetFirstN(limit, offset, "POST");
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

            foreach (var task in tasks)
            {
                response.AddEntities(task.Result.Entities.Where(e => e != null).ToList());
                foreach (var error in task.Result.Errors)
                {
                    response.AddKinveyException(new KinveyException(EnumErrorCategory.ERROR_BACKEND, EnumErrorCode.ERROR_GENERAL, error.Errmsg));
                }
                
            }

            response.PushCount = response.PushEntities.Count;

            return response;
        }

        private async Task<KinveyMultiInsertResponse<T>> HandlePushMultiPOST(ICollection <PendingWriteAction> pendingWriteActions)
        {
            var multiInsertNetworkResponse = new KinveyMultiInsertResponse<T>
            {
                Entities = new List<T>(),
                Errors = new List<Error>()
            };

            try
            {
                var localEntities = new List<Tuple<string, T, PendingWriteAction>>();

                foreach (var pendingWriteAction in pendingWriteActions)
                {
                    var entity = Cache.FindByID(pendingWriteAction.entityId);

                    var obj = JObject.FromObject(entity);
                    obj["_id"] = null;
                    entity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

                    localEntities.Add(new Tuple<string, T, PendingWriteAction>(pendingWriteAction.entityId, entity, pendingWriteAction));
                }

                var multiInsertNetworkRequest = Client.NetworkFactory.BuildMultiInsertRequest<T, KinveyMultiInsertResponse<T>>(Collection, localEntities.Select(e => e.Item2).ToList());
                multiInsertNetworkResponse = await multiInsertNetworkRequest.ExecuteAsync();

                for (var index = 0; index < localEntities.Count; index++)
                {
                    if (multiInsertNetworkResponse.Entities[index] != null)
                    {
                        Cache.UpdateCacheSave(multiInsertNetworkResponse.Entities[index], localEntities[index].Item1);
                        var result = SyncQueue.Remove(localEntities[index].Item3);
                    }
                }
            }
            catch (KinveyException ke)
            {
                response.AddKinveyException(ke);
            }

            return multiInsertNetworkResponse;
        }
    }
}
