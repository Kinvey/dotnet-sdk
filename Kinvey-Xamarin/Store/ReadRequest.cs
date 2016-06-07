using System;

namespace KinveyXamarin
{
	public abstract class ReadRequest <T, U> : Request <T, U>
	{
		ICache<T> Cache;
		string Collection;
		ReadPolicy Policy { get; }

		public ReadRequest (AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy)
			: base(client)
		{
			this.Cache = cache;
			this.Collection = collection;
			this.Policy = policy;
		}
	}
}

