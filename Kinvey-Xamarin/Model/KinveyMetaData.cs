using System;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	[JsonObject(MemberSerialization.OptIn)]
	public class KinveyMetaData
	{
		public KinveyMetaData ()
		{
		}

		public static string JSON_FIELD_NAME = "_kmd";

		[JsonProperty("lmt")]
		public String lastModifiedTime{get; set;}

		[JsonProperty("ect")]
		public String entityCreationTime{get; set;}



		[JsonObject(MemberSerialization.OptIn)]
		public class AccessControlList
		{

		}


	}
}

