using Newtonsoft.Json;
using SQLite;
using Kinvey;
using System.Runtime.Serialization;

namespace Kinvey.Tests
{
	[JsonObject(MemberSerialization.OptIn)]
    [DataContract]
	public class Address : IPersistable
	{
		[JsonProperty("_id")]
        [DataMember(Name = "_id")]
        [Preserve]
		[PrimaryKey, Column("_id")]
		public string ID { get; set; }

		[JsonProperty("_acl")]
        [DataMember(Name = "_acl")]
        [Preserve]
		[Column("_acl")]
        public AccessControlList Acl { get; set; }

		[JsonProperty("_kmd")]
        [DataMember(Name = "_kmd")]
        [Preserve]
		[Column("_kmd")]
		public KinveyMetaData Kmd { get; set; }

		[JsonProperty]
        [DataMember]
        public bool IsApartment { get; set; }

		[JsonProperty]
        [DataMember]
        public string Street { get; set; }
	}
}
