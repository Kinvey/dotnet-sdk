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
using System.Threading.Tasks;
using System.Linq;

namespace KinveyXamarin
{
	/// <summary>
	/// Request operation for pulling all records for a collection during a sync, and refreshing the cache with the
	/// updated data.
	/// </summary>
	public class PullRequest<T> : ReadRequest<T, List<T>>
	{
		public PullRequest(AbstractClient client, string collection, ICache<T> cache, bool deltaSetFetchingEnabled, IQueryable<T> query)
			: base(client, collection, cache, query, ReadPolicy.FORCE_NETWORK, deltaSetFetchingEnabled)
		{
		}

		public override async Task<List<T>> ExecuteAsync()
		{
			List<T> listResults = new List<T>();

			string mongoQuery = this.BuildMongoQuery();

			if (DeltaSetFetchingEnabled)
			{
				listResults = await PerformDeltaSetFetch(mongoQuery);
			}
			else
			{
				listResults = await Client.NetworkFactory.buildGetRequest<T>(Collection, mongoQuery).ExecuteAsync();
			}

			Cache.RefreshCache(listResults);

			return listResults;
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PullRequest not implemented.");
		}

		private async Task<List<T>> PerformDeltaSetFetch(string mongoQuery)
		{
			// TODO implement DSF
			List<T> listDeltaSetResults = new List<T>();
			List<string> listIDs = new List<string>();

			if (String.IsNullOrEmpty(mongoQuery))
			{
				mongoQuery = "{}";
			}

			mongoQuery += "&fields=_id, _kmd";

			// First, pull all members of a colletion, but IDs and LMTs only.
			List<DeltaSetFetchInfo> listNetworkEntities = new List<DeltaSetFetchInfo>();
			listNetworkEntities = await Client.NetworkFactory.buildGetRequest<DeltaSetFetchInfo>(Collection, mongoQuery).ExecuteAsync();


			// Second, compare to see what has been created, deleted and updated since the last pull

			// TODO find more efficient way of pulling just IDs/KMDs from database
			List<T> cacheItems = null;
			if (Query != null)
			{
				cacheItems = Cache.FindByQuery(Query.Expression);
			}
			else
			{
				cacheItems = Cache.FindAll();
			}
			Dictionary<string, string> cachedEntities = new Dictionary<string, string>();
			foreach (var cacheItem in cacheItems)
			{
				Entity item = cacheItem as Entity;
				cachedEntities.Add(item.ID, item.KMD.lastModifiedTime);
			}

			foreach (var networkEntity in listNetworkEntities)
			{
				string ID = networkEntity.ID;
				string LMT = networkEntity.KMD.lastModifiedTime;

				if (!cachedEntities.ContainsKey(ID))
				{
					// Case where a new item exists in the backend, but not in the local cache
					listIDs.Add(ID);
				}
				else if (cachedEntities.ContainsKey(ID) &&
						 HelperMethods.IsDateMoreRecent(LMT, cachedEntities[ID]))
				{
					// Case where the backend has a more up-to-date version of the entity than the local cache
					listIDs.Add(ID);
				}

				// NO-OPS
					// Case where a new item exists in the local cache, but not in the backend
					// Case where the local cache has a more up-to-date version of the entity than the backend

				// REMOVED ITEMS ???
					// Case where the backend has deleted an item that has not been removed from the local cache
					// Case where the local cache has deleted an item that has not been removed from the backend
			}



			// Then, with this set of IDs from the previous step, make a query to the
			// backend, to get full records for each ID that has changed since last fetch.
			int count = listIDs.Count();
			int start = 0;
			int batchSize = 1;

			while (start < count)
			{
				// TODO does the query only have to be run initially?  That is, once we have a list of IDs that we have
				// to update (which we got using the query), do we have to use the query again?
				string queryIDs = BuildIDsQuery(listIDs.GetRange(start, batchSize));
				List<T> listBatchResults = await Client.NetworkFactory.buildGetRequest<T>(Collection, queryIDs).ExecuteAsync();

				start += listBatchResults.Count();
				listDeltaSetResults.AddRange(listBatchResults);
			}

			return listDeltaSetResults;
		}

		private string BuildIDsQuery(List<string> listIDs)
		{
			System.Text.StringBuilder query = new System.Text.StringBuilder();

			query.Append("{\"_id\": { \"$in\": [");

			bool isNotFirstID = false;
			foreach (var ID in listIDs)
			{
				if (isNotFirstID)
				{
					query.Append(",");
				}

				query.Append("\"");
				query.Append(ID);
				query.Append("\"");

				isNotFirstID = true;
			}

			query.Append("] } }");

			return query.ToString();
		}
	}
}
