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
using System.Collections.Generic;
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
            KinveyDataStoreResponse<T> kinveyDataStoreResponse = default(KinveyDataStoreResponse<T>);

            switch (Policy)
            {
                case WritePolicy.FORCE_LOCAL:

                    kinveyDataStoreResponse = new KinveyDataStoreResponse<T>
                    {
                        Entities = new List<T>(),
                        Errors = new List<Error>()
                    };                    
                    var pendingWriteActions = new List<PendingWriteAction>();

                    if (entities.Count == 1)
                    {
                        //tempIdLocal = CacheSave(ref entity);
                    }
                    else
                    {
                        for (var index = 0; index < entities.Count; index++)
                        {
                            string tempIdLocal = null;
                            try
                            {
                                var entity = entities[index];
                                tempIdLocal = CacheSave(ref entity);
                                kinveyDataStoreResponse.Entities.Add(entity);
                            }
                            catch (Exception exception)
                            {
                                kinveyDataStoreResponse.Entities.Add(default(T));

                                var error = new Error
                                {
                                    Index = index,
                                    Code = 0,
                                    Errmsg = exception.Message
                                };

                                kinveyDataStoreResponse.Errors.Add(error);
                            }

                            var createRequest = Client.NetworkFactory.buildCreateRequest(Collection, entities[index]);

                            var pendingAction = PendingWriteAction.buildFromRequest(createRequest);
                            pendingAction.entityId = tempIdLocal;

                            pendingWriteActions.Add(pendingAction);
                        }
                    }

                    foreach (var pendingWriteAction in  pendingWriteActions)
                    {
                        SyncQueue.Enqueue(pendingWriteAction);
                    }

                    break;

                case WritePolicy.FORCE_NETWORK:
                    // network
                    var request = Client.NetworkFactory.buildMultiInsertRequest<T, KinveyDataStoreResponse<T>>(Collection, entities);
                    kinveyDataStoreResponse = await request.ExecuteAsync();
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


        private string CacheSave(ref T entity)
        {
            var tempIdLocal = PrepareCacheSave(ref entity);
            Cache.Save(entity);
            return tempIdLocal;
        }
    }
}