using System;

namespace KinveyXamarin
{
	public class Helpers
	{
		public Helpers ()
		{
		}


	}

	public class MongoCursor{

		public int Size ()
		{
			return 1;
		}
	}

	public class MongoCursor<T> : MongoCursor{

		public object Select<TSource, TResult> (Func<TSource, TResult> _projection)
		{
			throw new NotImplementedException ();
		}
	
	}
}

