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

namespace KinveyXamarin
{
	public class FindRequest<T> : ReadRequest<T, List<T>>
	{
		private List<string> EntityIDs { get; }
		private KinveyDelegate<List<T>> cacheDelegate;

		public FindRequest(AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, KinveyDelegate<List<T>> cacheDelegate, IQueryable<T> query, List<string> listIDs)
			: base(client, collection, cache, query, policy)
		{
			EntityIDs = listIDs;
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
					listResult = await PerformNetworkFind();
					break;

				case ReadPolicy.BOTH:
					// cache

					// first, perform local query
					PerformLocalFind(cacheDelegate);

					// once local query finishes, perform network query
					listResult = await PerformNetworkFind();
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

		private List<T> PerformLocalFind(KinveyDelegate<List<T>> localDelegate = null)
		{
			List<T> cacheHits = default(List<T>);

			try
			{
				if (Query != null)
				{
					IQueryable<T> query = Query;
					cacheHits = Cache.FindByQuery(query.Expression);
				}
				else if (EntityIDs?.Count > 0)
				{
					cacheHits = Cache.FindByIDs(EntityIDs);
				}
				else
				{
					cacheHits = Cache.FindAll();
				}

				localDelegate?.onSuccess(cacheHits);
			}
			catch (Exception e)
			{
				if (localDelegate != null)
				{
					localDelegate.onError(e);
				}
				else
				{
					throw e;
				}
			}

			return cacheHits;
		}

		private async Task<List<T>> PerformNetworkFind()
		{
			List<T> networkResults = default(List<T>);

			try
			{
				if (Query != null)
				{
					string mongoQuery = this.BuildMongoQuery();
					networkResults = await Client.NetworkFactory.buildGetRequest<T>(Collection, mongoQuery).ExecuteAsync();
				}
				else if (EntityIDs?.Count > 0)
				{
					networkResults = new List<T>();
					foreach (string entityID in EntityIDs)
					{
						T item = await Client.NetworkFactory.buildGetByIDRequest<T>(Collection, entityID).ExecuteAsync();
						networkResults.Add(item);
					}
				}
				else
				{
					networkResults = await Client.NetworkFactory.buildGetRequest<T>(Collection).ExecuteAsync();
				}
			}
			catch (KinveyException ke)
			{
				throw ke;
			}
			catch (Exception e)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK,
				                          EnumErrorCode.ERROR_GENERAL,
				                          "Error in FindAsync() for network results.",
				                          e);
			}

			return networkResults;
		}
	}
}
