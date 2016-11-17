using System;
using Kinvey;

namespace KinveyXamariniOS
{
	public static class PushExtension
	{
		public static Push Push (this Client client){
			return new Push (client);

		}
	}
}

