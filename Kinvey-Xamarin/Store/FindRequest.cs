using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Remotion.Linq;

namespace KinveyXamarin
{
	public class FindRequest<T> : ReadRequest<T, List<T>>
	{
		private List<string> EntityIDs { get; }
		private KinveyObserver<T> QueryObj { get; }
		private StringQueryBuilder Writer { get; }
		private IQueryable<T> Query { get; }

		public FindRequest(AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, KinveyObserver<T> queryObj, IQueryable<T> query, List<string> listIDs)
			: base(client, collection, cache, policy)
		{
			EntityIDs = listIDs;
			QueryObj = queryObj;
			Query = query;
			Writer = new StringQueryBuilder();
		}

		//public FindRequest(AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, KinveyObserver<T> queryObj, IQueryable<T> query, string entityID)
		//	: base(client, collection, cache, policy)
		//{
		//	List<string> listIDs = new List<string>();
		//	if (entityID != null)
		//	{
		//		listIDs.Add(entityID);
		//	}

		//	EntityIDs = listIDs;
		//	QueryObj = queryObj;
		//	Query = query;
		//	Writer = new StringQueryBuilder();
		//}

		public override async Task<List<T>> ExecuteAsync()
		{
			if (QueryObj == null)
			{
				throw new KinveyException(EnumErrorCode.ERROR_GENERAL, "FindRequest query object cannot be null");
			}

			List<T> listResult = default(List<T>);

			switch (Policy)
			{
				case ReadPolicy.FORCE_LOCAL:
					// sync
					try
					{
						PerformLocalFind();
					}
					catch (Exception e)
					{
						QueryObj.OnError(e);
					}
					break;

				case ReadPolicy.FORCE_NETWORK:
					// network
					try
					{
						await PerformNetworkFind();
					}
					catch (Exception e)
					{
						// network error
						QueryObj.OnError(e);
					}
					break;

				case ReadPolicy.BOTH:
					// cache
					try
					{
						// first, perform local query
						PerformLocalFind ();
					}
					catch (Exception e)
					{
						QueryObj.OnError(e);
					}

					try
					{
						// once local query finishes, perform network query
						await PerformNetworkFind ();
					}
					catch (Exception e)
					{
						QueryObj.OnError(e);
					}
					break;

				default:
					throw new KinveyException(EnumErrorCode.ERROR_GENERAL, "Invalid read policy");
			}

			return listResult;
		}

		public override async Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on FindRequest not implemented.");
		}

		private void PerformLocalFind()
		{
			List<T> cacheResults = default(List<T>);

			if (Query != null)
			{
				IQueryable<T> query = Query;
				cacheResults = Cache.FindByQuery(query.Expression);
			}
			else if (EntityIDs?.Count > 0)
			{
				cacheResults = Cache.FindByIDs(EntityIDs);
			}
			else
			{
				cacheResults = Cache.FindAll();
			}

			foreach (T cacheItem in cacheResults)
			{
				QueryObj.OnNext(cacheItem);
			}

			QueryObj.OnCompleted();
		}

		private async Task PerformNetworkFind()
		{
			List<T> networkResults = default(List<T>);

			if (Query != null)
			{
				Writer.Reset();

				KinveyQueryVisitor visitor = new KinveyQueryVisitor(Writer, typeof(T));

				QueryModel queryModel = (Query.Provider as KinveyQueryProvider).qm;

				Writer.Write("{");
				queryModel.Accept(visitor);
				Writer.Write("}");

				string mongoQuery = Writer.GetFullString();

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

			foreach (T networkItem in networkResults)
			{
				QueryObj.OnNext(networkItem);
			}

			QueryObj.OnCompleted();
		}
	}
}

