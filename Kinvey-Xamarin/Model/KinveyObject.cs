﻿
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

		[JsonProperty ("_acl")]
		[Preserve]
		[Column ("_acl")]
		public AccessControlList _acl { get; set; }

		[JsonProperty ("_kmd")]
		[Preserve]
		[Column("_kmd")]
		public KinveyMetaData _kmd { get; set; }
	}
}