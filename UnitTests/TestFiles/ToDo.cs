﻿using Newtonsoft.Json;
using Kinvey;

namespace UnitTestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ToDo : Entity
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty ("details")]
		public string Details { get; set; }

		[JsonProperty("due_date")]
		public string DueDate { get; set; }
	}
}

