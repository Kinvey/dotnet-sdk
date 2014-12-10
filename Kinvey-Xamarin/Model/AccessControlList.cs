using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace KinveyXamarin
{

	/// <summary>
	/// JSON represention of the _acl fied present on every stored in Kinvey
	/// </summary>
	[JsonObject]
	public class AccessControlList
	{

		/// <summary>
		/// the field name within every JSON object.
		/// </summary>
		public const string JSON_FIELD_NAME = "_acl";


		[JsonProperty]
		public string creator {get; set;}

		[JsonProperty("gr")]
		public bool globallyReadable {get; set;}

		[JsonProperty("gw")]
		public bool globallyWriteable {get; set;}

		[JsonProperty("r")]
		public List<string> read {get; set;}

		[JsonProperty("w")]
		public List<string> write {get; set;}

		[JsonProperty("groups")]
		public List<AclGroups> groups { get; set;}

		public AccessControlList(){}

		[JsonObject]
		public class AclGroups  {

			[JsonProperty("r")]
			public string read {get; set;}

			[JsonProperty("w")]
			public string write {get; set;}

			public AclGroups(){}

		}

	}

}

