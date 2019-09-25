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
using System.Linq;

namespace Kinvey
{
    /// <summary>
    /// Request operation for pushing of data from the library to the backend.
    /// </summary>
    /// <typeparam name="T">The type of an entity.</typeparam>
    public class PushRequest <T> : WriteRequest<T, PushDataStoreResponse<T>>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PushRequest{T}"/> class.
        /// </summary>
        /// <param name="client">Client that the user is logged in.</param>
        /// <param name="collection">Collection name.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="queue">Synchronization queue.</param>
        /// <param name="policy">Write policy.</param>
        public PushRequest(AbstractClient client, string collection, ICache<T> cache, ISyncQueue queue, WritePolicy policy)
			: base (client, collection, cache, queue, policy)
		{

		}

        /// <summary>
        /// Executes the request asynchronously.
        /// </summary>
        /// <returns> The async task with the request result.</returns>
        public override async Task <PushDataStoreResponse<T>> ExecuteAsync()
		{
            var response = new PushDataStoreResponse<T>();

            if (HelperMethods.IsLessThan(Client.ApiVersion, 5))
            {
                response = await PushSingleActionsAsync();
            }
            else
            {
                var pushMultiPostActionsResponse = await PushMultiPostActionsAsync();
                response.SetResponse(pushMultiPostActionsResponse);

                var pushSinglePutActionsResponse = await PushSingleActionsAsync("PUT");
                response.SetResponse(pushSinglePutActionsResponse);

                var pushSingleDeleteActionsResponse = await PushSingleActionsAsync("DELETE");
                response.SetResponse(pushSingleDeleteActionsResponse);
            }

            return response;
		}

        /// <summary>
        /// Communicates the request for cancellation.
        /// </summary>
        /// <returns>The async task with the boolean result. If the result is <c>true</c> then the request was canceled, otherwise <c>false</c>.</returns>
        public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PushRequest not implemented.");
		}

        #region Single actions pushes

        private async Task<PushDataStoreResponse<T>> PushSingleActionsAsync(string action = null)
        {
            var response = new PushDataStoreResponse<T>();
            var offset = 0;
            var limit = 10;

            var pendingActions = string.IsNullOrEmpty(action) ? SyncQueue.GetFirstN(limit, offset) :
                SyncQueue.GetFirstN(limit, offset, action);

            while (pendingActions != null && pendingActions.Count > 0)
            {
                var tasks = new List<Task<Tuple<T, KinveyException, int>>>();
                foreach (PendingWriteAction pwa in pendingActions)
                {
                    if (string.Equals("POST", pwa.action))
                    {
                        tasks.Add(HandlePushPOST(pwa));
                    }
                    else if (string.Equals("PUT", pwa.action))
                    {
                        tasks.Add(HandlePushPUT(pwa));
                    }
                    else if (string.Equals("DELETE", pwa.action))
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
                    response.AddKinveyException(new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK,
                                                                    EnumErrorCode.ERROR_JSON_RESPONSE,
                                                                    e.Message,
                                                                   e));
                }

                var resultEntities = new List<T>();
                var kinveyExceptions = new List<KinveyException>();
                var resultCount = 0;
                foreach (var task in tasks)
                {
                    if (!EqualityComparer<T>.Default.Equals(task.Result.Item1, default(T)))
                    {
                        resultEntities.Add(task.Result.Item1);
                    }

                    if (task.Result.Item2 != null)
                    {
                        kinveyExceptions.Add(task.Result.Item2);
                    }

                    offset += task.Result.Item3;

                    resultCount++;
                }

                response.AddEntities(resultEntities);
                response.AddExceptions(kinveyExceptions);
                response.PushCount += resultCount;

                pendingActions = string.IsNullOrEmpty(action) ? SyncQueue.GetFirstN(limit, offset) :
                SyncQueue.GetFirstN(limit, offset, action);
            }

            return response;
        }

        private async Task<Tuple<T, KinveyException, int>> HandlePushPOST(PendingWriteAction pwa)
        {
            T entity = default(T);
            var offset = 0;
            KinveyException kinveyException = null;

            try
            {
                int result = 0;

                string tempID = pwa.entityId;

                var localEntity = Cache.FindByID(pwa.entityId);

                JObject obj = JObject.FromObject(localEntity);
                obj["_id"] = null;
                localEntity = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ToString());

                NetworkRequest<T> request = Client.NetworkFactory.buildCreateRequest<T>(pwa.collection, localEntity);
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
                kinveyException = ke;
                offset++;
            }

            return new Tuple<T, KinveyException, int>(entity, kinveyException, offset);
        }

        private async Task<Tuple<T, KinveyException, int>> HandlePushPUT(PendingWriteAction pwa)
        {
            T entity = default(T);
            var offset = 0;
            KinveyException kinveyException = null;

            try
            {
                int result = 0;

                var localEntity = Cache.FindByID(pwa.entityId);

                NetworkRequest<T> request = Client.NetworkFactory.buildUpdateRequest<T>(pwa.collection, localEntity, pwa.entityId);
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
                kinveyException = ke;
                offset++;
            }

            return new Tuple<T, KinveyException, int>(entity, kinveyException, offset);
        }

        private async Task<Tuple<T, KinveyException, int>> HandlePushDELETE(PendingWriteAction pwa)
        {
            var offset = 0;
            KinveyException kinveyException = null;

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
                kinveyException = ke;
                offset++;
            }

            return new Tuple<T, KinveyException, int>(default(T), kinveyException, offset);
        }

        #endregion Single actions pushes

        #region Multiple actions pushes

        private async Task<PushDataStoreResponse<T>> PushMultiPostActionsAsync()
        {
            var response = new PushDataStoreResponse<T>();
            var limit = 10 * Constants.NUMBER_LIMIT_OF_ENTITIES;
            var offset = 0;
            var pendingPostActions = SyncQueue.GetFirstN(limit, offset, "POST");

            while (pendingPostActions != null && pendingPostActions.Count > 0)
            {
                var tasks = new List<Task<Tuple<PushDataStoreResponse<T>, int>>>();

                var realCountOfMultiInsertOperations = pendingPostActions.Count / (double)Constants.NUMBER_LIMIT_OF_ENTITIES;
                realCountOfMultiInsertOperations = Math.Ceiling(realCountOfMultiInsertOperations);

                for (var index = 0; index < realCountOfMultiInsertOperations; index++)
                {
                    var pendingWritePostActionsForPush = pendingPostActions.Skip(index * Constants.NUMBER_LIMIT_OF_ENTITIES).Take(Constants.NUMBER_LIMIT_OF_ENTITIES).ToList();

                    if (pendingWritePostActionsForPush.Count > 0)
                    {
                        tasks.Add(HandlePushMultiPOST(pendingWritePostActionsForPush));
                    }
                }

                await Task.WhenAll(tasks.ToArray());

                foreach (var task in tasks)
                {
                    response.AddEntities(task.Result.Item1.PushEntities);
                    response.AddExceptions(task.Result.Item1.KinveyExceptions);
                    offset += task.Result.Item2;
                }

                response.PushCount += pendingPostActions.Count;

                pendingPostActions = SyncQueue.GetFirstN(limit, offset, "POST");
            }
            return response;
        }

        private async Task<Tuple<PushDataStoreResponse<T>, int>> HandlePushMultiPOST(ICollection<PendingWriteAction> pendingWriteActions)
        {
            var offset = 0;
            var response = new PushDataStoreResponse<T>();

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

            if (!isException)
            {
                for (var index = 0; index < localData.Count; index++)
                {
                    try
                    {
                        if (multiInsertNetworkResponse.Entities[index] != null)
                        {
                            Cache.UpdateCacheSave(multiInsertNetworkResponse.Entities[index], localData[index].Item1);

                            var removeResult = SyncQueue.Remove(localData[index].Item3);

                            if (removeResult == 0)
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
            }

            var entities = multiInsertNetworkResponse.Entities.Where(e => e != null).ToList();
            response.AddEntities(entities);

            foreach (var error in multiInsertNetworkResponse.Errors)
            {
                response.AddKinveyException(new KinveyException(EnumErrorCategory.ERROR_BACKEND, EnumErrorCode.ERROR_GENERAL, error.Errmsg));
                offset++;
            }

            var result = new Tuple<PushDataStoreResponse<T>, int>(response, offset);

            return result;
        }

        #endregion Multiple actions pushes
    }
}
