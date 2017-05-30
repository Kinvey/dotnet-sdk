using Newtonsoft.Json;
using Kinvey;

namespace testiosapp2
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ToDo : Entity
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("details")]
		public string Details { get; set; }

		[JsonProperty("due_date")]
		public string DueDate { get; set; }

		[JsonProperty("value")]
		public int Value { get; set; }

		[JsonProperty("bool_value")]
		public bool BoolVal { get; set; }
	}
}

