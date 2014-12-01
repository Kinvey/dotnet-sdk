using System;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	/// <summary>
	/// Defines file meta data, storing arbitrary key/value pairs of data associated with a file.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class FileMetaData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.FileMetaData"/> class.
		/// </summary>
		public FileMetaData ()
		{
		}
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		[JsonProperty("_id")]
		public String id {get; set;}
	
		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		[JsonProperty("_filename")]
		public String fileName{get; set;}

		/// <summary>
		/// Gets or sets the size.
		/// </summary>
		/// <value>The size.</value>
		[JsonProperty("size")]
		public long size{get; set;}

		/// <summary>
		/// Gets or sets the mimetype.
		/// </summary>
		/// <value>The mimetype.</value>
		[JsonProperty("mimeType")]
		public String mimetype{get; set;}

		/// <summary>
		/// Gets or sets the Access Control List.
		/// </summary>
		/// <value>The acl.</value>
		[JsonProperty("_acl")]
		public KinveyMetaData.AccessControlList acl{get; set;}

		/// <summary>
		/// Gets or sets the upload URL.
		/// </summary>
		/// <value>The upload URL.</value>
		[JsonProperty("_uploadURL")]
		public String uploadUrl{get; set;}

		/// <summary>
		/// Gets or sets the download UR.
		/// </summary>
		/// <value>The download UR.</value>
		[JsonProperty("_downloadURL")]
		public String downloadURL{get; set;}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="KinveyXamarin.FileMetaData"/> is public.
		/// </summary>
		/// <value><c>true</c> if public; otherwise, <c>false</c>.</value>
		[JsonProperty("_public")]
		public bool _public {get; set;}
	}
}

