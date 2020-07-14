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
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq; 

namespace Kinvey
{
    /// <summary>
    /// Request built for use by a <see cref="DataStore{T}"/> to get count of entities.
    /// </summary>
    /// <typeparam name="T">The type of an entity.</typeparam>
	public class GetCountRequest<T> : ReadRequest<T, uint>
	{
        /// <summary>
		/// Initializes a new instance of the <see cref="GetCountRequest{T}"/> class.
		/// </summary>
		/// <param name="client">Client that the user is logged in.</param>
		/// <param name="collection">Collection name.</param>
		/// <param name="cache">Cache.</param>
		/// <param name="policy">Read policy.</param>
		/// <param name="deltaSetFetchingEnabled">If set to <c>true</c> delta set fetching enabled.</param>
		/// <param name="query">Query.</param>
		public GetCountRequest (AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, bool deltaSetFetchingEnabled, IQueryable<object> query)
			: base (client, collection, cache, query, policy, deltaSetFetchingEnabled)
		{
		}

        /// <summary>
        /// Executes the request asynchronously.
        /// </summary>
        /// <returns> The async task with the request result.</returns>
        public override async Task<uint> ExecuteAsync()
		{
			uint countResult = default(uint);

			switch (Policy)
			{
				case ReadPolicy.FORCE_LOCAL:
					// sync
					countResult = PerformLocalCount();
					break;

				case ReadPolicy.FORCE_NETWORK:
					// network
					countResult = await PerformNetworkCount().ConfigureAwait(false);
					break;

                case ReadPolicy.NETWORK_OTHERWISE_LOCAL:
                    // auto

                    KinveyException networkKinveyException = null;
                    try
                    {
                        // first, perform a network request
                        countResult = await PerformNetworkCount().ConfigureAwait(false);
                    }
                    catch (KinveyException exception)
                    {
                        if (exception.ErrorCategory != EnumErrorCategory.ERROR_DATASTORE_NETWORK || exception.ErrorCode != EnumErrorCode.ERROR_GENERAL)
                        {
                            throw;
                        }
                        networkKinveyException = exception;
                    }

                    // if the network request fails, fetch data from local cache
                    if (networkKinveyException != null)
                    {
                        countResult = PerformLocalCount();
                    }
                    break;

                default:
					throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_GENERAL, "Invalid read policy");
			}

			return countResult;
		}

        /// <summary>
        /// Communicates the request for cancellation.
        /// </summary>
        /// <returns>The async task with the boolean result. If the result is <c>true</c> then the request was canceled, otherwise <c>false</c>.</returns>
		public override async Task<bool> Cancel ()
		{
			throw new KinveyException (EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on GetCountRequest not implemented.");
		}

		private uint PerformLocalCount(KinveyDelegate<uint> localDelegate = null)
		{
			uint localCount = default(uint);

			try
			{
				if (Query != null)
				{
					var query = Query;
					localCount = (uint)Cache.CountByQuery(query.Expression);
				}
				else
				{
					localCount = (uint)Cache.CountAll();
				}

				localDelegate?.onSuccess(localCount);
			}
			catch (Exception e)
			{
				if (localDelegate != null)
				{
					localDelegate.onError(e);
				}
				else
				{
					throw;
				}
			}

			return localCount;
		}

		private async Task<uint> PerformNetworkCount()
		{
			uint networkCount = default(uint);

			try
			{
				string mongoQuery = this.BuildMongoQuery();
				NetworkRequest<JObject> request = Client.NetworkFactory.buildGetCountRequest<JObject>(Collection, mongoQuery);
				JObject networkResults = await request.ExecuteAsync().ConfigureAwait(false);

				if (networkResults != null)
				{
					JToken count = networkResults.GetValue("count");

					if (count != null)
					{
						networkCount = count.ToObject<uint>();
					}
					else
					{
						throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK,
												  EnumErrorCode.ERROR_GENERAL,
						                          "Error in FindCountAsync() for network results.");
					}
				}
			}
			catch (KinveyException)
			{
				throw;
			}
            catch (Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK,
										  EnumErrorCode.ERROR_GENERAL,
										  "Error in FindCountAsync() for network results.",
										  e);
			}

			return networkCount;
		}
	}
}
