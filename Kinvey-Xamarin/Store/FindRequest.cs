using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	public class FindRequest<T> : ReadRequest<T, List<T>>
	{
		private List<string> EntityIDs { get; }
		private KinveyDelegate<List<T>> cacheResults;

		public FindRequest(AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, KinveyDelegate<List<T>> cacheResults, IQueryable<T> query, List<string> listIDs)
			: base(client, collection, cache, query, policy)
		{
			EntityIDs = listIDs;
			this.cacheResults = cacheResults;
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
					try
					{
						// first, perform local query
						PerformLocalFind(cacheResults);
					}
					catch (Exception e)
					{
						//Observer.OnError(e);
					}

					try
					{
						// once local query finishes, perform network query
						listResult = await PerformNetworkFind();
					}
					catch (Exception e)
					{
						//Observer.OnError(e);
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

		private List<T> PerformLocalFind(KinveyDelegate<List<T>> intermediateResults = null)
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

				intermediateResults?.onSuccess(cacheHits);
			}
			catch (Exception e)
			{
				intermediateResults?.onError(e);
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
