using Newtonsoft.Json;
using KinveyXamarin;

namespace UnitTestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class LongData : Entity
	{
		[JsonProperty("seq")]
		public int Sequence { get; set; }

		[JsonProperty("first")]
		public string FirstName { get; set; }

		[JsonProperty("last")]
		public string LastName { get; set; }

		[JsonProperty("age")]
		public int Age { get; set; }

		[JsonProperty("street")]
		public string Street { get; set; }

		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("zip")]
		public string Zip { get; set; }

		[JsonProperty("dollar")]
		public string Dollar { get; set; }

		[JsonProperty("pick")]
		public string Pick { get; set; }

		[JsonProperty("paragraph")]
		public string Paragraph { get; set; }
	}
}
