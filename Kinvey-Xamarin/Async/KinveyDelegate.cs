using System;

namespace KinveyXamarin
{
	public class KinveyDelegate<T>
	{
		public Action<T> onSuccess;
		public Action<Exception> onError;

	}
}

