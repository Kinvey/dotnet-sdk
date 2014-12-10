using System;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	[JsonObject]
	public class KinveyFile
	{
		[JsonProperty("_type")]
		public string type {get; set;}

		[JsonProperty("_id")]
		public string id {get; set;}



		public KinveyFile(String id){
		this.type = "KinveyRef";
			this.id = id;
		}



	}
}

