using Newtonsoft.Json;
using Kinvey;

namespace Kinvey.Tests
{
	[JsonObject(MemberSerialization.OptIn)]
	public class FlashCard : Entity
	{
		[JsonProperty("question")]
		public string Question { get; set; }

		[JsonProperty ("answer")]
		public string Answer { get; set; }

		[JsonProperty("mastered")]
		public bool Mastered { get; set; }
	}
}
