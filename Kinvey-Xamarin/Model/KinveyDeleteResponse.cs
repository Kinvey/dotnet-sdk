using System;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyDeleteResponse
	{
		public KinveyDeleteResponse ()
		{}

		public int count{get; set;}
	}
}

