using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Remotion.Linq;
using Newtonsoft.Json.Linq; 

namespace KinveyXamarin
{
	public class GetCountRequest<T> : ReadRequest<T, uint>, IObservable<uint>
	{
		private IObserver<uint> Observer { get; set;}

		public GetCountRequest (AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, IQueryable<T> query)
			: base (client, collection, cache, query, policy)
		{
		}

		public IDisposable Subscribe (IObserver<uint> observer)
		{
			this.Observer = observer;
			return new Unsubscriber ();
		}

		private class Unsubscriber : IDisposable
		{
			public void Dispose () { }
		}


		public override async Task<uint> ExecuteAsync ()
		{
			if (Observer == null) {
				throw new KinveyException (EnumErrorCode.ERROR_GENERAL, "The observer to a GetCountRequest cannot be null.");
			}

			uint countResult = default (uint);

			switch (Policy) {
			case ReadPolicy.FORCE_LOCAL:
				// sync
				try {
					PerformLocalCount ();
				} catch (Exception e) {
					Observer.OnError (e);
				}
				break;

			case ReadPolicy.FORCE_NETWORK:
				// network
				try {
					await PerformNetworkCount ();
				} catch (Exception e) {
					// network error
					Observer.OnError (e);
				}
				break;

			case ReadPolicy.BOTH:
				// cache
				try {
					// first, perform local query
					PerformLocalCount ();
				} catch (Exception e) {
					Observer.OnError (e);
				}

				try {
					// once local query finishes, perform network query
					await PerformNetworkCount ();
				} catch (Exception e) {
					Observer.OnError (e);
				}
				break;

			default:
				throw new KinveyException (EnumErrorCode.ERROR_GENERAL, "Invalid read policy");
			}

			Observer.OnCompleted ();
			return countResult;
		}

		public override async Task<bool> Cancel ()
		{
			throw new KinveyException (EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on GetCountRequest not implemented.");
		}

		private void PerformLocalCount ()
		{
			uint cacheResults = default (uint);

			try {
				if (Query != null) {
					IQueryable<T> query = Query;
					cacheResults = (uint)Cache.FindByQuery (query.Expression).Count;
				} else {
					cacheResults = (uint)Cache.FindAll ().Count;
				}

				//foreach (T cacheItem in cacheResults) {
				//	Observer.OnNext (cacheItem);
				//}

				Observer.OnNext (cacheResults);
			} catch (Exception e) {
				Observer.OnError (e);
			}
		}

		private async Task PerformNetworkCount ()
		{
			string mongoQuery = this.BuildMongoQuery ();

			try {
				JObject networkResults = await Client.NetworkFactory.buildGetCountRequest<JObject> (Collection, mongoQuery).ExecuteAsync ();

				if (networkResults != null) {
					JToken count = networkResults.GetValue ("count");

					if (count != null) {
						Observer.OnNext (count.ToObject<uint> ());
					} else {
						Observer.OnError (new KinveyException (EnumErrorCode.ERROR_GENERAL, "Failed to read the count from the backend response."));
					}
				}
			} catch (Exception e) {
				Observer.OnError (e);
			}
		}
	}
}

