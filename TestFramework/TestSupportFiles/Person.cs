using Newtonsoft.Json;
using SQLite;
using Kinvey;

namespace TestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Person : IPersistable
	{
		[JsonProperty("_id")]
        [Kinvey.Preserve]
        [SQLite.Preserve]
        [PrimaryKey, Column("_id")]
		public string Id { get; set; }

		[JsonProperty("_acl")]
        [Kinvey.Preserve]
        [SQLite.Preserve]
        [Column("_acl")]
		public AccessControlList Acl { get; set; }

		[JsonProperty("_kmd")]
        [Kinvey.Preserve]
        [SQLite.Preserve]
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
