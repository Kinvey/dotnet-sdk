// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
using System.Linq;
using System.Threading.Tasks;

namespace Kinvey
{
	/// <summary>
	/// Find request built for use by a <see cref="KinveyXamarin.DataStore{T}"/>
	/// </summary>
	public class FindRequest<T> : ReadRequest<T, List<T>>
	{

		private KinveyDelegate<List<T>> cacheDelegate;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:KinveyXamarin.FindRequest`1"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="cache">Cache.</param>
		/// <param name="policy">Policy.</param>
		/// <param name="deltaSetFetchingEnabled">If set to <c>true</c> delta set fetching enabled.</param>
		/// <param name="cacheDelegate">Cache delegate.</param>
		/// <param name="query">Query.</param>
		/// <param name="listIDs">List identifier.</param>
		public FindRequest(AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, bool deltaSetFetchingEnabled, KinveyDelegate<List<T>> cacheDelegate, IQueryable<object> query, List<string> listIDs)
			: base(client, collection, cache, query, policy, deltaSetFetchingEnabled, listIDs)
		{
			this.cacheDelegate = cacheDelegate;
		}

		public override async Task<List<T>> ExecuteAsync()
		{
			List<T> listResult = default(List<T>);

			switch (Policy)
			{
				case ReadPolicy.FORCE_LOCAL:
					// sync
					listResult = PerformLocalFind();
					break;

				case ReadPolicy.FORCE_NETWORK:
					// network
					var result = await RetrieveNetworkResults(this.BuildMongoQuery());
					listResult = result;
					break;

				case ReadPolicy.BOTH:
					// cache

					// first, perform local query
					PerformLocalFind(cacheDelegate);

					// once local query finishes, perform network query
					var resolved = await PerformNetworkFind();
					if (resolved.IsDeltaFetched)
					{
						listResult = PerformLocalFind();
					}
					else 
					{
						listResult = resolved.ResultSet;
					}
					break;

                case ReadPolicy.NETWORK_OTHERWISE_LOCAL:
                    // auto

                    KinveyException networkKinveyException = null;
                    try
                    {
                        // first, perform a network request
                        var networkResult = await PerformNetworkFind();
                        if (networkResult.IsDeltaFetched)
                        {
                            listResult = PerformLocalFind();
                        }
                        else
                        {
                            listResult = networkResult.ResultSet;
                        }
                    }
                    catch (KinveyException exception)
                    {
                        exception.ThrowIfNotCorrespond(exception, EnumErrorCategory.ERROR_DATASTORE_NETWORK, EnumErrorCode.ERROR_NETWORK_CONNECTION_FAILED);
                        networkKinveyException = exception;
                    }

                    // if the network request fails, fetch data from local cache
                    if (networkKinveyException != null)
                    {
                        listResult = PerformLocalFind();
                    }
                    break;

                default:
					throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_GENERAL, "Invalid read policy");
			}

			return listResult;
		}

		public override async Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on FindRequest not implemented.");
		}
	}
}
