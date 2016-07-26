using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq; 

namespace KinveyXamarin
{
	public class GetCountRequest<T> : ReadRequest<T, uint>
	{
		private KinveyDelegate<uint> cacheCount;

		public GetCountRequest (AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, KinveyDelegate<uint> cacheCount, IQueryable<T> query)
			: base (client, collection, cache, query, policy)
		{
		}

		public override async Task<uint> ExecuteAsync()
		{
			uint countResult = default(uint);

			switch (Policy)
			{
				case ReadPolicy.FORCE_LOCAL:
					// sync
					countResult = PerformLocalCount();
					break;

				case ReadPolicy.FORCE_NETWORK:
					// network
					countResult = await PerformNetworkCount();
					break;

				case ReadPolicy.BOTH:
					// cache

					// first, perform local query
					PerformLocalCount(cacheCount);

					// once local query finishes, perform network query
					countResult = await PerformNetworkCount();
					break;

				default:
					throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_GENERAL, "Invalid read policy");
			}

			return countResult;
		}

		public override async Task<bool> Cancel ()
		{
			throw new KinveyException (EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on GetCountRequest not implemented.");
		}

		private uint PerformLocalCount(KinveyDelegate<uint> intermediateCount = null)
		{
			uint localCount = default(uint);

			try
			{
				if (Query != null)
				{
					IQueryable<T> query = Query;
					localCount = (uint)Cache.FindByQuery(query.Expression).Count;
				}
				else
				{
					localCount = (uint)Cache.FindAll().Count;
				}

				intermediateCount?.onSuccess(localCount);
			}
			catch (Exception e)
			{
				if (intermediateCount != null)
				{
					intermediateCount.onError(e);
				}
				else
				{
					throw e;
				}
			}

			return localCount;
		}

		private async Task<uint> PerformNetworkCount()
		{
			uint networkCount = default(uint);

			try
			{
				string mongoQuery = this.BuildMongoQuery();
				NetworkRequest<JObject> request = Client.NetworkFactory.buildGetCountRequest<JObject>(Collection, mongoQuery);
				JObject networkResults = await request.ExecuteAsync();

				if (networkResults != null)
				{
					JToken count = networkResults.GetValue("count");

					if (count != null)
					{
						networkCount = count.ToObject<uint>();
					}
					else
					{
						throw new KinveyException(EnumErrorCategory.ERROR_DATASTORE_NETWORK,
												  EnumErrorCode.ERROR_GENERAL,
						                          "Error in FindCountAsync() for network results.");
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
										  "Error in FindCountAsync() for network results.",
										  e);
			}

			return networkCount;
		}
	}
}

