using System;
using RestSharp;
using Newtonsoft.Json;
using KinveyXamarin;
using System.Collections.Generic;
using Android.Graphics;


namespace AndroidStatusShare
{
	[JsonObject(MemberSerialization.OptIn)]
	public class UpdateEntity
	{
		public UpdateEntity ()
		{
		}

		[JsonProperty("_id")]
		public string ID {get; set;}

		[JsonProperty]
		public string text{get;set;}

		[JsonProperty("_kmd")]
		public KinveyMetaData kmd;

		[JsonProperty("_acl")]
		public KinveyMetaData.AccessControlList acl;

		[JsonProperty]
		public KinveyReference author;

		[JsonProperty]
		public List<KinveyReference> comments;


		//-----displayed inferred fields
		public string authorName;
		public string authorID;
		public string since;
		public Bitmap thumbnail;


		public String getWhen(){
			return "--";
			//TODO calcalate time since _kmd.ect
		}


	}
}