using Newtonsoft.Json;
using SQLite.Net.Attributes;
using KinveyXamarin;

namespace UnitTestFramework
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
		public AccessControlList ACL { get; set; }

		[JsonProperty("_kmd")]
		[Preserve]
		[Column("_kmd")]
		public KinveyMetaData KMD { get; set; }

		[JsonProperty]
		public string FirstName { get; set; }

		[JsonProperty]
		public string LastName { get; set; }

		[JsonProperty]
		public Address MailAddress { get; set; }

		[JsonProperty]
		public int Age { get; set; }

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
