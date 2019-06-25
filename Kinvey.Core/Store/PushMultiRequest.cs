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
        private PushDataStoreResponse<T> response;
        private int offset = 0;

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
            var limit = 10 * Constants.NUMBER_LIMIT_OF_ENTITIES;
            var pendingPostActions = SyncQueue.GetFirstN(limit, offset, "POST");
           
            while (pendingPostActions != null && pendingPostActions.Count > 0)
            {
                var tasks = new List<Task<KinveyMultiInsertResponse<T>>>();

                var realCountOfMultiInsertOperations = pendingPostActions.Count / (double)Constants.NUMBER_LIMIT_OF_ENTITIES;
               realCountOfMultiInsertOperations = Math.Ceiling(realCountOfMultiInsertOperations);

                for (var index = 0; index < realCountOfMultiInsertOperations; index++)
                {
                    var pendingWritePostActionsForPush = pendingPostActions.Skip(index * Constants.NUMBER_LIMIT_OF_ENTITIES).Take(Constants.NUMBER_LIMIT_OF_ENTITIES).ToList();

                    if (pendingWritePostActionsForPush.Count > 0)
                    {
                        tasks.Add(HandlePushMultiPOST(pendingWritePostActionsForPush));
                        //await Task.Delay(1000);
                    }
                }

                await Task.WhenAll(tasks.ToArray());

                foreach (var task in tasks)
                {
                    var entities = task.Result.Entities.Where(e => e != null).ToList();

                    response.AddEntities(entities);
                    foreach (var error in task.Result.Errors)
                    {
                        response.AddKinveyException(new KinveyException(EnumErrorCategory.ERROR_BACKEND, EnumErrorCode.ERROR_GENERAL, error.Errmsg));
                    }
                }

                pendingPostActions = SyncQueue.GetFirstN(limit, offset, "POST");
            }

            response.PushCount = response.PushEntities.Count;

            return response;
        }

        private async Task<KinveyMultiInsertResponse<T>> HandlePushMultiPOST(ICollection<PendingWriteAction> pendingWriteActions)
        {
            var multiInsertNetworkResponse = new KinveyMultiInsertResponse<T>
            {
                Entities = new List<T>(),
                Errors = new List<Error>()
            };

            var localData = new List<Tuple<string, T, PendingWriteAction>>();
            var isException = false;

            try
            {
                foreach (var pendingWriteAction in pendingWriteActions)
                {
                    var entity = Cache.FindByID(pendingWriteAction.entityId);

                    var obj = JObject.FromObject(entity);
                    obj["_id"] = null;
                    entity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

                    localData.Add(new Tuple<string, T, PendingWriteAction>(pendingWriteAction.entityId, entity, pendingWriteAction));
                }
               
                var multiInsertNetworkRequest = Client.NetworkFactory.BuildMultiInsertRequest<T, KinveyMultiInsertResponse<T>>(Collection, localData.Select(e => e.Item2).ToList());
                multiInsertNetworkResponse = await multiInsertNetworkRequest.ExecuteAsync();

            }
            catch (KinveyException ke)
            {
                response.AddKinveyException(ke);
                offset += pendingWriteActions.Count;
                isException = true;
            }
            catch (Exception ex)
            {
                response.AddKinveyException(new KinveyException(EnumErrorCategory.ERROR_GENERAL,
                                                                EnumErrorCode.ERROR_GENERAL,
                                                                ex.Message,
                                                               ex));
                offset += pendingWriteActions.Count;
                isException = true;
            }

            if (isException)
            {
                return multiInsertNetworkResponse;
            }

            for (var index = 0; index < localData.Count; index++)
            {
                try
                {
                    if (multiInsertNetworkResponse.Entities[index] != null)
                    {
                        Cache.UpdateCacheSave(multiInsertNetworkResponse.Entities[index], localData[index].Item1);

                        //throw new Exception("Test");

                        var result = SyncQueue.Remove(localData[index].Item3);

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
                catch (Exception ex)
                {
                    response.AddKinveyException(new KinveyException(EnumErrorCategory.ERROR_GENERAL,
                                                                    EnumErrorCode.ERROR_GENERAL,
                                                                    ex.Message,
                                                                   ex));
                    offset++;
                }
            }

            return multiInsertNetworkResponse;
        }
    }
}
