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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kinvey
{
    /// <summary>
    /// Represents a multi insert request. 
    /// </summary>
    public class MultiInsertRequest<T> : WriteRequest<T, KinveyMultiInsertResponse<T>>
    {
        private IList<T> entities;

        public MultiInsertRequest(IList<T> entities, AbstractClient client, string collection, ICache<T> cache, ISyncQueue sync, WritePolicy policy)
            : base(client, collection, cache, sync, policy)
        {
            this.entities = entities;
        }

        /// <summary>
        /// Executes a multi insert request.
        /// </summary>
        /// <returns>An async task with the request result.</returns>
        public override async Task<KinveyMultiInsertResponse<T>> ExecuteAsync()
        {
            var kinveyDataStoreResponse = new KinveyMultiInsertResponse<T>
            {
                Entities = new List<T>(),
                Errors = new List<Error>()
            };

            switch (Policy)
            {
                case WritePolicy.FORCE_LOCAL:
                    //local cache                   
                    for (var index = 0; index < entities.Count; index++)
                    {
                        try
                        {
                            var cacheSaveResult = CacheSave(entities[index]);
                            SyncQueue.Enqueue(cacheSaveResult.Item1);
                            kinveyDataStoreResponse.Entities.Add(cacheSaveResult.Item2);
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

                    if (kinveyDataStoreResponse.Entities.FindAll(item => item != null).Count == 0 && kinveyDataStoreResponse.Errors.Count > 0)
                    {
                        throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_MULTIPLE_SAVE, string.Empty);
                    }

                    break;

                case WritePolicy.FORCE_NETWORK:
                    // network
                    kinveyDataStoreResponse = await HandleNetworkRequestAsync(entities);
                    break;

                case WritePolicy.LOCAL_THEN_NETWORK:
                    //local cache
                    KinveyMultiInsertResponse<T> kinveyDataStoreNetworkResponse = null;

                    var pendingWriteActions = new List<PendingWriteAction>();

                    for (var index = 0; index < entities.Count; index++)
                    {
                        try
                        {
                            var cacheSaveResult = CacheSave(entities[index]);
                            pendingWriteActions.Add(cacheSaveResult.Item1);
                            kinveyDataStoreResponse.Entities.Add(cacheSaveResult.Item2);                            
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

                    if (kinveyDataStoreResponse.Entities.FindAll(item => item != null).Count == 0 && kinveyDataStoreResponse.Errors.Count > 0)
                    {
                        throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_CACHE, EnumErrorCode.ERROR_DATASTORE_CACHE_MULTIPLE_SAVE, string.Empty);
                    }

                    HttpRequestException exception = null;
                    try
                    {
                        // network
                        kinveyDataStoreNetworkResponse = await HandleNetworkRequestAsync(entities);
                    }
                    catch (HttpRequestException httpRequestException)
                    {
                        exception = httpRequestException;
                    }

                    if (exception != null)
                    {
                        foreach (var pendingAction in pendingWriteActions)
                        {
                            SyncQueue.Enqueue(pendingAction);
                        }                       
                    }
                    else 
                    {
                        for (var index = 0; index < kinveyDataStoreResponse.Entities.Count; index++)
                        {
                            if (kinveyDataStoreNetworkResponse.Entities[index] != null)
                            {                               
                                if (kinveyDataStoreResponse.Entities[index] != null)
                                {
                                    var obj = JObject.FromObject(kinveyDataStoreResponse.Entities[index]);
                                    var id = obj["_id"].ToString();
                                    Cache.UpdateCacheSave(kinveyDataStoreNetworkResponse.Entities[index], id);
                                }
                                else
                                {
                                    CacheSave(kinveyDataStoreNetworkResponse.Entities[index]);
                                }
                            }
                            else
                            {
                                if (kinveyDataStoreResponse.Entities[index] != null)
                                {
                                    var obj = JObject.FromObject(kinveyDataStoreResponse.Entities[index]);
                                    var id = obj["_id"].ToString();

                                    var existingPendingWriteAction = pendingWriteActions.Find(e => e.entityId.Equals(id));

                                    if (existingPendingWriteAction!= null)
                                    {
                                        SyncQueue.Enqueue(existingPendingWriteAction);
                                    }
                                }
                            }
                        }

                        kinveyDataStoreResponse = kinveyDataStoreNetworkResponse;
                    }

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

        private Tuple<PendingWriteAction, T> CacheSave(T entity)
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

            return new Tuple<PendingWriteAction, T>(pendingAction, savedEntity);
        }

        private async Task<KinveyMultiInsertResponse<T>> HandleNetworkRequestAsync(IList<T> entities)
        {
            var kinveyDataStoreResponse = new KinveyMultiInsertResponse<T>
            {
                Entities = HelperMethods.Initialize<T>(default(T), entities.Count),
                Errors = new List<Error>()
            };

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

            var multiInsertNetworkResponse = new KinveyMultiInsertResponse<T>
            {
                Entities = new List<T>(),
                Errors = new List<Error>()
            };

            if (entitiesToMultiInsert.Count > 0)
            {
                var countOfMultiInsertOperations = Math.Ceiling(entitiesToMultiInsert.Count / (double)Constants.NUMBER_LIMIT_OF_ENTITIES);
                var currentIndex = 0;
                var currentCountOfMultiInsertOperations = 0;

                while (currentCountOfMultiInsertOperations < countOfMultiInsertOperations)
                {
                    var tasks = new List<Task<KinveyMultiInsertResponse<T>>>();

                    for (var index = currentCountOfMultiInsertOperations; index < currentCountOfMultiInsertOperations + 10; index++)
                    {
                        if (index < countOfMultiInsertOperations)
                        {
                            tasks.Add(HandleMultiInsertRequestAsync(entitiesToMultiInsert.Skip(index * Constants.NUMBER_LIMIT_OF_ENTITIES).Take(Constants.NUMBER_LIMIT_OF_ENTITIES).ToList()));
                        }
                    }

                    await Task.WhenAll(tasks.ToArray());
                   
                    foreach (var task in tasks)
                    {
                        for (var index = 0; index < task.Result.Entities.Count; index++)
                        {
                            multiInsertNetworkResponse.Entities.Add(task.Result.Entities[index]);
                            if (task.Result.Entities[index] == null)
                            {
                                var error = task.Result.Errors?.Find(er => er.Index == index);

                                if (error != null)
                                {
                                    var newError = new Error
                                    {
                                        Index = currentIndex,
                                        Code = error.Code,
                                        Errmsg = error.Errmsg
                                    };
                                    multiInsertNetworkResponse.Errors.Add(newError);
                                }
                            }
                            currentIndex++;
                        }
                    }

                    currentCountOfMultiInsertOperations += 10;
                }
            }

            for (var index = 0; index < multiInsertNetworkResponse.Entities.Count; index++)
            {
                kinveyDataStoreResponse.Entities[initialIndexes[index]] = multiInsertNetworkResponse.Entities[index];

                var error = multiInsertNetworkResponse.Errors?.Find(er => er.Index == index);

                if (error != null)
                {
                    var newError = new Error
                    {
                        Index = initialIndexes[index],
                        Code = error.Code,
                        Errmsg = error.Errmsg
                    };
                    kinveyDataStoreResponse.Errors.Add(newError);
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

            kinveyDataStoreResponse.Errors.Sort((x, y) => x.Index.CompareTo(y.Index));

            if (kinveyDataStoreResponse.Entities.All(e => e == null) && kinveyDataStoreResponse.Errors.Count > 0)
            {
                throw new KinveyException(EnumErrorCategory.ERROR_BACKEND, EnumErrorCode.ERROR_JSON_RESPONSE, kinveyDataStoreResponse.Errors[0].Errmsg);
            }

            return kinveyDataStoreResponse;
        }

        private async Task<KinveyMultiInsertResponse<T>> HandleMultiInsertRequestAsync(IList<T> entities)
        {
            var response = new KinveyMultiInsertResponse<T>();

            try
            {
                var multiInsertRequest = Client.NetworkFactory.BuildMultiInsertRequest<T, KinveyMultiInsertResponse<T>>(Collection, entities.ToList());
                response = await multiInsertRequest.ExecuteAsync();
            }
            catch (KinveyException exeption)
            {
                if (exeption.StatusCode == 500)
                {
                    response.Entities = new List<T>();
                    response.Errors = new List<Error>();
                    for (var index = 0; index < entities.Count(); index++)
                    {
                        response.Entities.Add(default(T));
                        response.Errors.Add(new Error { Code = 0, Errmsg = exeption.Message, Index = index });
                    }
                }
                else
                {
                    throw;
                }
            }

            return response;
        }
    }
}