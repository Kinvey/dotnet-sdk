using System;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	abstract public class Request <T, U>
	{
		protected AbstractClient Client { get;} 

		public Request (AbstractClient client)
		{
			this.Client = client;

		}

		public abstract Task<U> ExecuteAsync ();

		public abstract Task<bool> Cancel();
	}
}

