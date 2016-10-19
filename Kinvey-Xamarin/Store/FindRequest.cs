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
	/// <summary>
	/// Find request built for use by a <see cref="KinveyXamarin.DataStore{T}"/>
	/// </summary>
	public class FindRequest<T> : ReadRequest<T, List<T>>
	{
		private List<string> EntityIDs { get; }
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
			: base(client, collection, cache, query, policy, deltaSetFetchingEnabled)
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
					Cache.RefreshCache(listResult);
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
					IQueryable<object> query = Query;
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
				string mongoQuery = this.BuildMongoQuery();

				if (DeltaSetFetchingEnabled && !Cache.IsCacheEmpty())
				{
					networkResults = await PerformDeltaSetFetch(mongoQuery);
				}
				else
				{
					if (Query != null)
					{
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

					Cache.Clear(Query?.Expression);
				}

				Cache.RefreshCache(networkResults);
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

			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();

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

			stopwatch.Stop();
			stopwatch.Reset();

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
