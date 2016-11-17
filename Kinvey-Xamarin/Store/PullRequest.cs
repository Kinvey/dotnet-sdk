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

namespace Kinvey
{
	/// <summary>
	/// Request operation for pulling all records for a collection during a sync, and refreshing the cache with the
	/// updated data.
	/// </summary>
	public class PullRequest<T> : ReadRequest<T, PullDataStoreResponse<T>>
	{
		public PullRequest(AbstractClient client, string collection, ICache<T> cache, bool deltaSetFetchingEnabled, IQueryable<object> query)
			: base(client, collection, cache, query, ReadPolicy.FORCE_NETWORK, deltaSetFetchingEnabled)
		{
		}

		public override async Task<PullDataStoreResponse<T>> ExecuteAsync()
		{
			PullDataStoreResponse<T> response = new PullDataStoreResponse<T>();
			List<T> listResults = new List<T>(); // TODO remove??

			string mongoQuery = this.BuildMongoQuery();

			if (DeltaSetFetchingEnabled && !Cache.IsCacheEmpty())
			{
				listResults = await PerformDeltaSetFetch(mongoQuery);
			}
			else
			{
				listResults = await Client.NetworkFactory.buildGetRequest<T>(Collection, mongoQuery).ExecuteAsync();
				Cache.Clear(Query?.Expression);
			}

			Cache.RefreshCache(listResults);

			response.AddEntities(listResults);
			response.PullCount = listResults.Count;

			return response;
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PullRequest not implemented.");
		}

		private async Task<List<T>> PerformDeltaSetFetch(string mongoQuery)
		{
			List<T> listDeltaSetResults = new List<T>();

			#region DSF Step 1: Pull all entity IDs and LMTs of a collection in the backend

			// TODO Need to paginate in case there are more than 10,0000 entities in the collection
			if (String.IsNullOrEmpty(mongoQuery))
			{
				mongoQuery = "{}";
			}

			mongoQuery += "&fields=_id,_kmd.lmt";

			List<DeltaSetFetchInfo> listNetworkEntities = new List<DeltaSetFetchInfo>();
			listNetworkEntities = await Client.NetworkFactory.buildGetRequest<DeltaSetFetchInfo>(Collection, mongoQuery).ExecuteAsync();

			#endregion

			#region DSF Step 2: Pull all entity IDs and LMTs of a collection in local storage

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

			Dictionary<string, string> dictCachedEntities = new Dictionary<string, string>();

			foreach (var cacheItem in cacheItems)
			{
				Entity item = cacheItem as Entity;
				dictCachedEntities.Add(item.ID, item.KMD.lastModifiedTime);
			}

			List<string> listCachedEntitiesToRemove = new List<string>(dictCachedEntities.Keys);

			#endregion

			#region DSF Step 3: Compare backend and local entities to see what has been created, deleted and updated since the last fetch

			List<string> listIDsToFetch = new List<string>();

			foreach (var networkEntity in listNetworkEntities)
			{
				string ID = networkEntity.ID;
				string LMT = networkEntity.KMD.lastModifiedTime;

				if (!dictCachedEntities.ContainsKey(ID))
				{
					// Case where a new item exists in the backend, but not in the local cache
					listIDsToFetch.Add(ID);
				}
				else if (HelperMethods.IsDateMoreRecent(LMT, dictCachedEntities[ID]))
				{
					// Case where the backend has a more up-to-date version of the entity than the local cache
					listIDsToFetch.Add(ID);
				}

				// Case where the backend has deleted an item that has not been removed from local storage.
				//
				// To begin with, this list has all the IDs currently present in local storage.  If an ID
				// has been found in the set of backend IDs, we will remove it from this list.  What will
				// remain in this list are all the IDs that are currently in local storage that
				// are not present in the backend, and therefore have to be deleted from local storage.
				listCachedEntitiesToRemove.Remove(ID);

				// NO-OPS: Should never hit these cases, because a Push() has to happen prior to a pull
				// 		Case where a new item exists in the local cache, but not in the backend
				// 		Case where the local cache has a more up-to-date version of the entity than the backend
				// 		Case where the local cache has deleted an item that has not been removed from the backend
			}

			#endregion

			#region DSF Step 4: Remove items from local storage that are no longer in the backend

			Cache.DeleteByIDs(listCachedEntitiesToRemove);

			#endregion

			#region DSF Step 5: Fetch selected IDs from backend to update local storage

			// Then, with this set of IDs from the previous step, make a query to the
			// backend, to get full records for each ID that has changed since last fetch.
			int numIDs = listIDsToFetch.Count();
			int start = 0;
			int batchSize = 200;

			while (start < numIDs)
			{
				int count = Math.Min((numIDs - start), batchSize);
				string queryIDs = BuildIDsQuery(listIDsToFetch.GetRange(start, count));
				List<T> listBatchResults = await Client.NetworkFactory.buildGetRequest<T>(Collection, queryIDs).ExecuteAsync();

				start += listBatchResults.Count();
				listDeltaSetResults.AddRange(listBatchResults);
			}

			#endregion

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

			// TODO need to add back in any modifiers from original query

			return query.ToString();
		}
	}
}
