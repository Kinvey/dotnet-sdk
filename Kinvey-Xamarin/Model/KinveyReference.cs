using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KinveyXamarin
{
	[JsonObject]
	public class KinveyReference<T>
	{
	
		[JsonProperty]
		public string type {get; set;}

		[JsonProperty("_id")]
		public string id {get; set;}

		[JsonProperty("_collection")]
		public string collection {get; set;}

		[JsonProperty("_obj")]
		public T resolved {get; set;}

		public KinveyReference ()
		{
			this.type = "KinveyRef";
		}

		public KinveyReference(string collection, string id)
		{
			this.type = "KinveyRef";
			this.collection = collection;
			this.id = id;
		}

		public T getResolvedObject(){
			return resolved;


		}
	}
}

