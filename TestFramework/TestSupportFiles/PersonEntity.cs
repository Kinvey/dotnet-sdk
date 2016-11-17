using Newtonsoft.Json;
using SQLite.Net.Attributes;
using KinveyXamarin;

namespace TestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class PersonEntity : Entity
	{
		[JsonProperty]
		public string FirstName { get; set; }

		[JsonProperty]
		public string LastName { get; set; }

		[JsonProperty]
		public AddressEntity MailAddress { get; set; }
	}
}
