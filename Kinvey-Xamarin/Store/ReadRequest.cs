using System;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	public abstract class ReadRequest <T, U> : Request <T, U>
	{
		public ICache<T> Cache;
		public string Collection;
		public ReadPolicy Policy { get; }

		public ReadRequest (AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy)
			: base(client)
		{
			this.Cache = cache;
			this.Collection = collection;
			this.Policy = policy;
		}
	}
}

