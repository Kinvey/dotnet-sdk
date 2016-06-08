using System;
using Newtonsoft.Json;
using KinveyXamarin;

namespace UnitTestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class FlashCard
	{
		[JsonProperty("_id")]
		public string ID {get; set;}

		[JsonProperty("question")]
		public string Question { get; set; }

		[JsonProperty ("answer")]
		public string Answer { get; set; }

		[JsonProperty("mastered")]
		public bool Mastered { get; set; }

		[JsonProperty("_kmd")]
		public KinveyMetaData Metadata { get; set; }
	}
}

