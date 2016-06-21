
using System;
using Newtonsoft.Json;
using SQLite.Net.Attributes;
namespace KinveyXamarin
{
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyObject
	{
		[JsonProperty ("_id")]
		[Preserve]
		[PrimaryKey, Column("_id")]
		public string ID { get; set; }
	}
}