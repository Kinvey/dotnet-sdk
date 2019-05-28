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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kinvey
{
    /// <summary>
    /// Represents a multi insert request. 
    /// </summary>
    public class MultiInsertRequest<T> : WriteRequest<T, KinveyDataStoreResponse<T>>
    {
        private List<T> entities;

        public MultiInsertRequest(List<T> entities, AbstractClient client, string collection, ICache<T> cache, ISyncQueue sync, WritePolicy policy)
            : base(client, collection, cache, sync, policy)
        {
            this.entities = entities;
        }

        /// <summary>
        /// Executes a multi insert request.
        /// </summary>
        /// <returns>An async task with the request result.</returns>
        public override async Task<KinveyDataStoreResponse<T>> ExecuteAsync()
        {
            var kinveyDataStoreResponse = new KinveyDataStoreResponse<T>
            {
                Entities = HelperMethods.Initialize<T>(default(T), entities.Count),
                Errors = new List<Error>()
            };

            switch (Policy)
            {
                case WritePolicy.FORCE_LOCAL:
                                       
                    var pendingWriteActions = new List<PendingWriteAction>();

                    if (entities.Count == 1)
                    {
                        CacheSave(entities[0], kinveyDataStoreResponse);                                                           
                    }
                    else
                    {
                        for (var index = 0; index < entities.Count; index++)
                        {
                            try
                            {
                                CacheSave(entities[index], kinveyDataStoreResponse);
                            }
                            catch (Exception ex)
                            {
                                kinveyDataStoreResponse.Entities.Add(default(T));

                                var error = new Error
                                {
                                    Index = index,
                                    Code = 0,
                                    Errmsg = ex.Message
                                };
                                kinveyDataStoreResponse.Errors.Add(error);
                            }
                        }
                    }

                    break;

                case WritePolicy.FORCE_NETWORK:
                    // network

                    var updateRequests = new Dictionary<int, NetworkRequest<T>>();

                    var entitiesToMultiInsert = new List<T>();
                    var initialIndexes = new List<int>();

                    for (var index = 0; index < entities.Count; index++)
                    {
                        var idToken = JObject.FromObject(entities[index])["_id"];
                        if (idToken != null && !string.IsNullOrEmpty(idToken.ToString()))
                        {
                            var updateRequest = Client.NetworkFactory.buildUpdateRequest(Collection, entities[index], idToken.ToString());
                            updateRequests.Add(index, updateRequest);
                        }
                        else
                        {
                            entitiesToMultiInsert.Add(entities[index]);
                            initialIndexes.Add(index);
                        }
                    }

                    var multiInsertRequest = Client.NetworkFactory.buildMultiInsertRequest<T, KinveyDataStoreResponse<T>>(Collection, entitiesToMultiInsert);
                    var multiInsertKinveyDataStoreResponse = await multiInsertRequest.ExecuteAsync();

                    for (var index = 0; index < multiInsertKinveyDataStoreResponse.Entities.Count; index++)
                    {
                        kinveyDataStoreResponse.Entities[initialIndexes[index]] = multiInsertKinveyDataStoreResponse.Entities[index];

                        var error = multiInsertKinveyDataStoreResponse.Errors?.Find(er => er.Index == index);

                        if(error != null)
                        {
                            error.Index = initialIndexes[index];
                            kinveyDataStoreResponse.Errors.Add(error);
                        }
                    }

                    foreach (var updateRequest in updateRequests)
                    {
                        T updatedEntity = default(T);
                        try
                        {
                            updatedEntity = await updateRequest.Value.ExecuteAsync();
                        }
                        catch (Exception ex)
                        {
                            var error = new Error
                            {
                                Index = updateRequest.Key,
                                Code = 0,
                                Errmsg = ex.Message
                            };

                            kinveyDataStoreResponse.Errors.Add(error);
                        }

                        kinveyDataStoreResponse.Entities[updateRequest.Key] = updatedEntity;
                    }
                    break;

                case WritePolicy.LOCAL_THEN_NETWORK:

                    //var pendingWriteActions2 = new List<PendingWriteAction>();

                    //var dictionary = new Dictionary<string, T>();

                    //if (entities.Count == 1)
                    //{
                    //    var tempIdLocal = CacheSave(entities[0], kinveyDataStoreResponse);
                    //    dictionary.Add(tempIdLocal, entities[0]);
                    //}
                    //else
                    //{
                    //    for (var index = 0; index < entities.Count; index++)
                    //    {
                    //        try
                    //        {
                    //            var tempIdLocal = CacheSave(entities[index], kinveyDataStoreResponse);
                    //            dictionary.Add(tempIdLocal, entities[index]);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            kinveyDataStoreResponse.Entities.Add(default(T));

                    //            var error = new Error
                    //            {
                    //                Index = index,
                    //                Code = 0,
                    //                Errmsg = ex.Message
                    //            };

                    //            kinveyDataStoreResponse.Errors.Add(error);
                    //        }                           
                    //    }
                    //}

                    //HttpRequestException exception = null;
                    //try
                    //{
                    //    // network save
                    //    var request2 = Client.NetworkFactory.buildMultiInsertRequest<T, KinveyDataStoreResponse<T>>(Collection, entities);
                    //    kinveyDataStoreResponse = await request2.ExecuteAsync();
                    //}
                    //catch (HttpRequestException httpRequestException)
                    //{
                    //    exception = httpRequestException;
                    //}

                    //if (exception != null)
                    //{
                    //    foreach (var item in dictionary)
                    //    {
                    //        var pendingAction = CreatePendingWriteAction(item.Key, item.Value);
                    //        SyncQueue.Enqueue(pendingAction);
                    //    }
                    //}
                    //else
                    //{
                    //    foreach (var item in kinveyDataStoreResponse.Entities)
                    //    {
                    //        //Cache.UpdateCacheSave(savedEntity, tempIdLocalThenNetwork);
                    //    }
                    //}



                    break;

                default:
                    throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_GENERAL, "Invalid write policy");
            }
            
            return kinveyDataStoreResponse;
        }

        /// <summary>
        /// Cancels a multi insert request.
        /// </summary>
        /// <returns>An async task with a boolean result.</returns>
        public override Task<bool> Cancel()
        {
            throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on MultiInsertRequest not implemented.");
        }


        //private string CacheSave(T entity, KinveyDataStoreResponse<T> kinveyDataStoreResponse)
        //{
        //    var tempIdLocal = PrepareCacheSave(ref entity);
        //    Cache.Save(entity);
        //    kinveyDataStoreResponse.Entities.Add(entities[0]);
        //    return tempIdLocal;
        //}

        private void CacheSave(T entity, KinveyDataStoreResponse<T> kinveyDataStoreResponse)
        {
            PendingWriteAction pendingAction = null;
            T savedEntity = default(T);

            var idToken = JObject.FromObject(entity)["_id"];
            if (idToken != null && !String.IsNullOrEmpty(idToken.ToString()))
            {
                var networkRequest = Client.NetworkFactory.buildUpdateRequest(Collection, entity, idToken.ToString());

                savedEntity = Cache.Update(entity);

                pendingAction = PendingWriteAction.buildFromRequest(networkRequest);             
            }
            else
            {
                var networkRequest = Client.NetworkFactory.buildCreateRequest(Collection, entity);

                var tempIdLocal = PrepareCacheSave(ref entity);
                savedEntity = Cache.Save(entity);
                
                pendingAction = PendingWriteAction.buildFromRequest(networkRequest);
                pendingAction.entityId = tempIdLocal;
            }

            SyncQueue.Enqueue(pendingAction);
            kinveyDataStoreResponse.Entities.Add(savedEntity);
        }

        private PendingWriteAction CreatePendingWriteAction(string localTempId, T entity)
        {
            var createRequest = Client.NetworkFactory.buildCreateRequest(Collection, entity);

            var pendingAction = PendingWriteAction.buildFromRequest(createRequest);
            pendingAction.entityId = localTempId;

            return pendingAction;
        }
    }
}