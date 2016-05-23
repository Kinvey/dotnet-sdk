using System;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	abstract public class Request <T>
	{
		private ICache<T> Cache {get; }
		private AbstractClient Client { get;} 

		public abstract Task<T> ExecuteAsync ();
	}
}

