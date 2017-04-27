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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;

namespace Kinvey
{
	public abstract class ReadRequest <T, U> : Request <T, U>
	{
		public ICache<T> Cache { get; }
		public string Collection { get; }
		public ReadPolicy Policy { get; }
		protected IQueryable<object> Query { get; }
		protected bool DeltaSetFetchingEnabled { get; }
		protected List<string> EntityIDs { get; }

		public ReadRequest(AbstractClient client, string collection, ICache<T> cache, IQueryable<object> query, ReadPolicy policy, bool deltaSetFetchingEnabled)
	: base(client)
		{
			this.Cache = cache;
			this.Collection = collection;
			this.Query = query;
			this.Policy = policy;
			this.DeltaSetFetchingEnabled = deltaSetFetchingEnabled;
		}


		public ReadRequest (AbstractClient client, string collection, ICache<T> cache, IQueryable<object> query, ReadPolicy policy, bool deltaSetFetchingEnabled, List<String> entityIds)
			: base(client)
		{
			this.Cache = cache;
			this.Collection = collection;
			this.Query = query;
			this.Policy = policy;
			this.DeltaSetFetchingEnabled = deltaSetFetchingEnabled;
			this.EntityIDs = entityIds;
		}

		protected string BuildMongoQueryForDelta() {
			var lastEntity = Cache.LastModifiedEntity(Query?.Expression) as IPersistable;
			if (lastEntity == null) {
				return BuildMongoQuery();
			}

			string deltaQuery = "\"_kmd.lmt\": { \"$gt\": \"" + lastEntity.KMD.lastModifiedTime + "\"}";

			if (Query != null)
			{
				StringQueryBuilder queryBuilder = new StringQueryBuilder();

				KinveyQueryVisitor visitor = new KinveyQueryVisitor(queryBuilder, typeof(T));
				QueryModel queryModel = (Query.Provider as KinveyQueryProvider)?.qm;

				queryModel?.Accept(visitor);
				queryBuilder.AddQueryExpression(deltaQuery);

				string mongoQuery = queryBuilder.BuildQueryString();

				return mongoQuery;
			}

			var q = new StringQueryBuilder();
			q.AddQueryExpression(deltaQuery);

			return q.BuildQueryString();

		}

		/// <summary>
		/// Builds the mongo-style query string to be run against the backend.
		/// </summary>
		/// <returns>The mongo-style query string.</returns>
		protected string BuildMongoQuery()
		{
			if (Query != null)
			{
				StringQueryBuilder queryBuilder = new StringQueryBuilder();

				KinveyQueryVisitor visitor = new KinveyQueryVisitor(queryBuilder, typeof(T));
				QueryModel queryModel = (Query.Provider as KinveyQueryProvider)?.qm;

				queryModel?.Accept(visitor);

				string mongoQuery = queryBuilder.BuildQueryString();
				return mongoQuery;
			}

			return default (string);
		}

		protected List<T> PerformLocalFind(KinveyDelegate<List<T>> localDelegate = null)
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

		protected async Task<NetworkReadResponse<T>> PerformNetworkFind()
		{
			try
			{
				if (DeltaSetFetchingEnabled && !Cache.IsCacheEmpty())
				{

					var query = BuildMongoQueryForDelta();

					var delta = await Client.NetworkFactory.buildGetRequest<T>(Collection, query).ExecuteAsync();
					if (delta.Count > 0) {
						Cache.RefreshCache(delta);
					}

					var deletedItems = await Client.NetworkFactory.buildGetRequest<Entity>(Collection + "-deleted").ExecuteAsync();
					if (deletedItems.Count > 0) { 
						Cache.DeleteByIDs(deletedItems.Select(x => x.ID).ToList());
					}

					System.Diagnostics.Debug.WriteLine("delta size: " + delta.Count + "  delete size: " + deletedItems.Count);
					//TODO: total count should be returned in this response
					return new NetworkReadResponse<T>(delta, 0, true);
				
				}

				string mongoQuery = this.BuildMongoQuery();

				var results = await RetrieveNetworkResults(mongoQuery);
				Cache.Clear(Query?.Expression);
				Cache.RefreshCache(results);				
				return new NetworkReadResponse<T>(results, results.Count, false);
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
		}

		protected async Task<List<T>> RetrieveNetworkResults(string mongoQuery) 
		{
			List<T> networkResults = default(List<T>);

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

			return networkResults;
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

		protected class NetworkReadResponse<T>
		{
			public List<T> ResultSet;
			public int TotalCount;
			public bool IsDeltaFetched;

			public NetworkReadResponse(List<T> result, int count, bool isDelta)
			{
				this.ResultSet = result;
				this.TotalCount = count;
				this.IsDeltaFetched = isDelta;
			}
		}

	}
}
