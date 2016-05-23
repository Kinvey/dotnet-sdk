using System;

namespace KinveyXamarin
{
	public abstract class ReadRequest <T, U> : Request <T, U>
	{
		ICache<T> Cache;
		string Collection;

		public ReadRequest (AbstractClient client, string collection, ICache<T> cache): base(client)
		{
			this.Cache = cache;
			this.Collection = collection;
		}

	}
}

