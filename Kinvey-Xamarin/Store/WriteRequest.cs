using System;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	public abstract class WriteRequest <T, U> : Request <T, U>
	{
		public ISyncQueue SyncQueue { get;}
		public ICache<T> Cache { get; } 
		public string Collection { get; }

		public WriteRequest (AbstractClient client, string collection, ICache<T> cache, ISyncQueue queue): base (client){
			this.Collection = collection;
			this.Cache = cache;
			this.SyncQueue = queue;
		}

	}
}

	