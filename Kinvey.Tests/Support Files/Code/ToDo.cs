using System;
using Newtonsoft.Json;

namespace Kinvey.Tests
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

		[JsonProperty("new_date")]
		public DateTime NewDate { get; set; }

		[JsonProperty("value")]
		public int Value { get; set; }

		[JsonProperty("bool_value")]
		public bool BoolVal { get; set; }

        [JsonProperty("_geoloc")]
        public string GeoLoc { get; set; }
    }
}

