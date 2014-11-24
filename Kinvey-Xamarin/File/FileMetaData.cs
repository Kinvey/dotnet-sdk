using System;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	[JsonObject(MemberSerialization.OptIn)]
	public class FileMetaData
	{
		public FileMetaData ()
		{
		}


		[JsonProperty("_id")]
		public String id {get; set;}
	
		[JsonProperty("_filename")]
		public String fileName{get; set;}

		[JsonProperty("size")]
		public long size{get; set;}

		[JsonProperty("mimeType")]
		public String mimetype{get; set;}

		[JsonProperty("_acl")]
		public KinveyMetaData.AccessControlList acl{get; set;}

		[JsonProperty("_uploadURL")]
		public String uploadUrl{get; set;}

		[JsonProperty("_downloadURL")]
		public String downloadURL{get; set;}

		[JsonProperty("_public")]
		public bool _public {get; set;}
	}
}

