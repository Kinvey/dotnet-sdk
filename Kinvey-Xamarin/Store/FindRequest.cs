using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Remotion.Linq;

namespace KinveyXamarin
{
	public class FindRequest<T> : ReadRequest<T, List<T>>, IObservable<List<T>>
	{
		private List<string> EntityIDs { get; }
		private IObserver<List<T>> Observer { get; set; }

		public FindRequest(AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, IQueryable<T> query, List<string> listIDs)
			: base(client, collection, cache, query, policy)
		{
			EntityIDs = listIDs;
			//Observer = queryObj;

		}
		public IDisposable Subscribe (IObserver<List<T>> observer)
		{
			this.Observer = observer;
			return new Unsubscriber ();
		}

		private class Unsubscriber : IDisposable
		{
			public void Dispose ()
			{
			}
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
			if (Observer == null)
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
						Observer.OnError(e);
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
						Observer.OnError(e);
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
						Observer.OnError(e);
					}

					try
					{
						// once local query finishes, perform network query
						await PerformNetworkFind ();
					}
					catch (Exception e)
					{
						Observer.OnError(e);
					}
					break;

				default:
					throw new KinveyException(EnumErrorCode.ERROR_GENERAL, "Invalid read policy");
			}

			Observer.OnCompleted ();
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

			//foreach (T cacheItem in cacheResults)
			//{
			//	Observer.OnNext(cacheItem);
			//}

			Observer.OnNext (cacheResults);
			//Observer.OnCompleted();
		}

		private async Task PerformNetworkFind()
		{
			List<T> networkResults = default(List<T>);

			if (Query != null) { 
				string mongoQuery = this.BuildMongoQuery ();
				networkResults = await Client.NetworkFactory.buildGetRequest<T> (Collection, mongoQuery).ExecuteAsync ();
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

			Observer.OnNext (networkResults);

		}

	}
}

