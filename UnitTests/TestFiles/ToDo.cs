using System;
using Newtonsoft.Json;
using KinveyXamarin;

namespace UnitTestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ToDo
	{
		[JsonProperty("_id")]
		public string ID {get; set;}

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty ("details")]
		public string Details { get; set; }

		[JsonProperty("due_date")]
		public string DueDate { get; set; }

		[JsonProperty("_kmd")]
		public KinveyMetaData Metadata { get; set; }
	}
}

