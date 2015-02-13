using System;
using KinveyXamarin;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace KinveyXamarinAndroid
{
	public static class PushExtension
	{
		public static Push Push (this Client client){
			return new Push (client);

		}





	}
}

