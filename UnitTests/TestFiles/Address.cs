using Newtonsoft.Json;
using SQLite.Net.Attributes;
using KinveyXamarin;

namespace UnitTestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Address : IPersistable
	{
		[JsonProperty("_id")]
		[Preserve]
		[Column("_id")]
		public string ID { get; set; }

		[JsonProperty("_acl")]
		[Preserve]
		[Column("_acl")]
		public AccessControlList ACL { get; set; }

		[JsonProperty("_kmd")]
		[Preserve]
		[Column("_kmd")]
		public KinveyMetaData KMD { get; set; }

		public bool IsApartment { get; set; }

		public string Street { get; set; }

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
