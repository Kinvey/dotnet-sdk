// Copyright (c) 2015, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

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
		[JsonProperty(AccessControlList.JSON_FIELD_NAME)]
		public AccessControlList acl{get; set;}

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

