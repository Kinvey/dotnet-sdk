using Newtonsoft.Json;
using SQLite;
using Kinvey;
using System.Runtime.Serialization;

namespace TestFramework
{
	[JsonObject(MemberSerialization.OptIn)]
    [DataContract]
	public class Address : IPersistable
	{
		[JsonProperty("_id")]
        [DataMember(Name = "_id")]
		[Kinvey.Preserve]
        [SQLite.Preserve]
        [PrimaryKey, Column("_id")]
		public string Id { get; set; }

		[JsonProperty("_acl")]
        [DataMember(Name = "_acl")]
        [Kinvey.Preserve]
        [SQLite.Preserve]
        [Column("_acl")]
		public AccessControlList Acl { get; set; }

		[JsonProperty("_kmd")]
        [DataMember(Name = "_kmd")]
        [Kinvey.Preserve]
        [SQLite.Preserve]
        [Column("_kmd")]
		public KinveyMetaData KMD { get; set; }

		[JsonProperty]
        [DataMember]
        public bool IsApartment { get; set; }

		[JsonProperty]
        [DataMember]
        public string Street { get; set; }
	}
}
