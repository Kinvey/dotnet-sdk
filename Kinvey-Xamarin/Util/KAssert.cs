using System;

namespace KinveyXamarin
{
	public class KAssert
	{
		public KAssert ()
		{
		}

		public static String notNull(string toAssert, string message){
			if (toAssert == null) {
				throw new ArgumentNullException (message);
			}
			return toAssert;
		}
	}
}

