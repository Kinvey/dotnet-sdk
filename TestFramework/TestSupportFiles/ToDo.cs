﻿using Newtonsoft.Json;
using Kinvey;

namespace TestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ToDo : Entity, IStreamable
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty ("details")]
		public string Details { get; set; }

		[JsonProperty("due_date")]
		public string DueDate { get; set; }

		[JsonProperty("SenderID")]
		public string SenderID { get; set; }
	}
}

