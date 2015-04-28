using System;
using KinveyXamarin;

namespace KinveyXamariniOS
{
	public static class PushExtension
	{
		public static Push Push (this Client client){
			return new Push (client);

		}
	}
}

