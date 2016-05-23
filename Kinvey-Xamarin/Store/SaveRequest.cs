using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	public class SaveRequest <T> : WriteRequest<T, T>
	{
		private T entity; 

		public SaveRequest (T entity, AbstractClient client, string collection, ICache<T> cache, ISyncQueue sync)
			: base (client, collection, cache, sync)
		{}

		public override async Task<T> ExecuteAsync ()
		{
			//T saved = await this.Cache.SaveAsync (entity);
			NetworkRequest<T> request = Client.NetworkFactory.buildCreateRequest (Collection, entity);
			//int result = await this.SyncQueue.Enqueue (PendingWriteAction.buildFromRequest <T> (request);

			//PendingWriteAction action = await this.SyncQueue.Pop ();
			return await request.ExecuteAsync ();
		}

		public override Task<bool> Cancel ()
		{
			throw new NotImplementedException ();
		}
	}
}

