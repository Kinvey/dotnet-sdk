using System;

namespace KinveyXamarin
{
	public abstract class KinveyDelegate<T>
	{
		public abstract Action<T> onSuccess(T entity);
		public abstract Action<Exception> onError(Exception error);

	}
}

