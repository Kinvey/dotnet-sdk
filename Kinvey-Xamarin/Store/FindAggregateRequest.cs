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
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	/// <summary>
	/// Find request built in order to apply grouping/aggregation functions to entities within a <see cref="KinveyXamarin.DataStore{T}"/>
	/// </summary>
	public class FindAggregateRequest<T> : ReadRequest<T, int>
	{
		private KinveyDelegate<int> cacheDelegate;
		private EnumReduceFunction reduceFunction;
		private string propertyName;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:KinveyXamarin.FindAggregateRequest`1"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="collection">Collection.</param>
		/// <param name="reduceFunction">Reduce function.</param>
		/// <param name="cache">Cache.</param>
		/// <param name="policy"> The <see cref="ReadPolicy"/> to be used for this request.</param>
		/// <param name="deltaSetFetchingEnabled">If set to <c>true</c> delta set fetching enabled.</param>
		/// <param name="cacheDelegate">Cache delegate.</param>
		/// <param name="query">[optional] Query used to filter the results that are to be aggregated.</param>
		/// <param name="propertyName">Property name to be used for aggregation.</param>
		public FindAggregateRequest(AbstractClient client,
		                            string collection,
		                            EnumReduceFunction reduceFunction,
		                            ICache<T> cache,
		                            ReadPolicy policy,
		                            bool deltaSetFetchingEnabled,
		                            KinveyDelegate<int> cacheDelegate,
		                            IQueryable<object> query,
		                            string propertyName)
			: base(client, collection, cache, query, policy, deltaSetFetchingEnabled)
		{
			this.cacheDelegate = cacheDelegate;
			this.reduceFunction = reduceFunction;
			this.propertyName = propertyName;
		}

		/// <summary>
		/// Executes the request asynchronously.
		/// </summary>
		/// <returns>The async.</returns>
		public override async Task<int> ExecuteAsync()
		{
			int aggregateResult = default(int);

			switch (Policy)
			{
				case ReadPolicy.FORCE_LOCAL:
					// sync
					aggregateResult = PerformLocalAggregateFind();
					break;

				case ReadPolicy.FORCE_NETWORK:
					// network
					aggregateResult = await PerformNetworkAggregateFind();
					break;

				case ReadPolicy.BOTH:
					// cache

					// first, perform local aggregation
					PerformLocalAggregateFind(cacheDelegate);

					// once local finishes, perform network aggregation
					aggregateResult = await PerformNetworkAggregateFind();
					break;

				default:
					throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_GENERAL, "Invalid read policy");
			}

			return aggregateResult;
		}

		public override async Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on GetCountRequest not implemented.");
		}

		private int PerformLocalAggregateFind(KinveyDelegate<int> localDelegate = null)
		{
			// TODO implement
			int localResult = default(int);

			try
			{
				if (Query != null)
				{
					IQueryable<object> query = Query;
					localResult = (int)Cache.FindByQuery(query.Expression).Count;
				}
				else
				{
					localResult = (int)Cache.FindAll().Count;
				}

				localDelegate?.onSuccess(localResult);
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

			return localResult;
		}

		private async Task<int> PerformNetworkAggregateFind()
		{
			int networkResult = default(int);

			try
			{
				string mongoQuery = this.BuildMongoQuery();
				NetworkRequest<JArray> request = Client.NetworkFactory.BuildGetAggregateRequest<JArray>(Collection, reduceFunction, mongoQuery, propertyName);
				JArray networkResults = await request.ExecuteAsync();

				if (networkResults != null)
				{
					foreach (JToken obj in networkResults)
					{
						JToken result = (obj as JObject).GetValue("result");

						if (result != null)
						{
							networkResult = result.ToObject<int>();
						}
						else
						{
							throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK,
													  EnumErrorCode.ERROR_GENERAL,
													  "Error in FindAggregateAsync() for network results.");
						}
					}
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
										  "Error in FindAggregateAsync() for network results.",
										  e);
			}

			return networkResult;
		}
	}
}
