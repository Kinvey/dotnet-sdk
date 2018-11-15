using Newtonsoft.Json;
using SQLite;
using Kinvey;

namespace Kinvey.Tests
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Person : IPersistable
	{
		[JsonProperty("_id")]
		[Preserve]
		[PrimaryKey, Column("_id")]
		public string ID { get; set; }

		[JsonProperty("_acl")]
		[Preserve]
		[Column("_acl")]
		public AccessControlList Acl { get; set; }

		[JsonProperty("_kmd")]
		[Preserve]
		[Column("_kmd")]
		public KinveyMetaData Kmd { get; set; }

		[JsonProperty]
		public string FirstName { get; set; }

		[JsonProperty]
		public string LastName { get; set; }

		[JsonProperty]
		public Address MailAddress { get; set; }

		[JsonProperty]
		public int Age { get; set; }
	}
}
