using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace KinveyXamarin
{
	/// <summary>
	/// This class provides access to Kinvey's File API.
	/// </summary>
	public class File
	{
		/// <summary>
		/// Gets or sets the client.
		/// </summary>
		/// <value>The client.</value>
		private AbstractClient client {get; set;}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.File"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		public File (AbstractClient client)
		{
			this.client = client;
		}

		/// <summary>
		/// Downloads the file associated with the _id contained in the FileMetaData.
		/// </summary>
		/// <returns>a blocking Download request</returns>
		/// <param name="metadata">Metadata.</param>
		public DownloadMetadataAndFile downloadBlocking(FileMetaData metadata)
		{

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", KAssert.notNull(metadata.id, "metadata.id is required to download a specific file-- can also download by query if _id is unknown."));

			DownloadMetadataAndFile download = new DownloadMetadataAndFile (urlParameters, this.client);

			client.InitializeRequest (download);


			//TODO need more elegant approach for mime type
//			RestSharp.HttpHeader mime = new RestSharp.HttpHeader ();
//			mime.Name  = "x-kinvey-content-type";
//			mime.Value = "application/octet-stream";
//			download.RequestHeaders.Add (mime);

			return download;
		}


		/// <summary>
		/// Uploads the FileMetaData and it's associated file.
		/// </summary>
		/// <returns>a blocking upload request.</returns>
		/// <param name="metadata">Metadata.</param>
		public UploadMetadataAndFile uploadBlocking(FileMetaData metadata)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			string mode = "POST";
			if (metadata.id != null && metadata.id.Length > 0) {
				mode = "PUT";
			}

			UploadMetadataAndFile upload = new UploadMetadataAndFile (metadata, mode, urlParameters, this.client);

			client.InitializeRequest (upload);
		
			return upload;
		}


		/// <summary>
		/// Downloads the FileMetaData blocking.
		/// </summary>
		/// <returns>The blocking download Request.</returns>
		/// <param name="fileId">File _id.</param>
		public DownloadMetadata downloadMetadataBlocking(String fileId)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", KAssert.notNull(fileId, "fileId is required to download metadata for a specific file."));

			DownloadMetadata download = new DownloadMetadata (urlParameters, this.client);

			client.InitializeRequest (download);

			return download;

		}


		/// <summary>
		/// Uploads the metadata.
		/// </summary>
		/// <returns>The blocking upload request.</returns>
		/// <param name="metadata">Metadata.</param>
		public UploadMetadata uploadMetadataBlocking(FileMetaData metadata)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", KAssert.notNull(metadata.id, "metadata.id is required to download metadata for a specific file."));

			UploadMetadata upload = new UploadMetadata (metadata, urlParameters, this.client);

			client.InitializeRequest (upload);

			return upload;

		}

		/// <summary>
		/// Deletes a file's FileMetaData
		/// </summary>
		/// <returns>The blocking delete request.</returns>
		/// <param name="fileId">File _id.</param>
		public DeleteMetadataAndFile deleteBlocking(String fileId)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", KAssert.notNull(fileId, "fileId is required to download metadata for a specific file."));

			DeleteMetadataAndFile delete = new DeleteMetadataAndFile (urlParameters, this.client);

			client.InitializeRequest (delete);

			return delete;



		}





		/// <summary>
		/// A synchronously request to download metadata and file.
		/// </summary>
		[JsonObject(MemberSerialization.OptIn)]
		public class DownloadMetadataAndFile : KinveyFileRequest
		{

			private const string REST_PATH = "blob/{appKey}/{fileID}";

			[JsonProperty]
			public string fileID { get; set;}

			public DownloadMetadataAndFile(Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "GET", REST_PATH, default(FileMetaData), urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}
				

		}

		/// <summary>
		/// A synchronously request to upload metadata and file.
		/// </summary>
		[JsonObject(MemberSerialization.OptIn)]
		public class UploadMetadataAndFile : KinveyFileRequest
		{

			private const string REST_PATH = "blob/{appKey}";

			[JsonProperty]
			public string fileID { get; set;}

			public UploadMetadataAndFile(FileMetaData meta, string mode, Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, mode, REST_PATH, meta, urlProperties)
			{

				if (mode.Equals("PUT"))
				{
					this.fileID = urlProperties["fileID"];
					this.uriTemplate += "/{fileID}";
				}
			}
		}

		/// <summary>
		/// A synchronously request to download metadata.
		/// </summary>
		[JsonObject(MemberSerialization.OptIn)]
		public class DownloadMetadata : KinveyFileRequest
		{

			private const string REST_PATH = "blob/{appKey}/{fileID}";

			[JsonProperty]
			public string fileID { get; set;}

			public DownloadMetadata(Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "GET", REST_PATH, default(FileMetaData), urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}


		}

		/// <summary>
		/// A synchronously request to upload metadata.
		/// </summary>
		[JsonObject(MemberSerialization.OptIn)]
		public class UploadMetadata : KinveyFileRequest
		{

			private const string REST_PATH = "blob/{appKey}/{fileID}";

			[JsonProperty]
			public string fileID { get; set;}

			public UploadMetadata(FileMetaData meta, Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "PUT", REST_PATH, meta, urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}
		}

		/// <summary>
		/// A synchronously request to delete metadata and file.
		/// </summary>
		[JsonObject(MemberSerialization.OptIn)]
		public class DeleteMetadataAndFile : AbstractKinveyClientRequest<KinveyDeleteResponse>
		{

			private const string REST_PATH = "blob/{appKey}/{fileID}";

			[JsonProperty]
			public string fileID { get; set;}

			public DeleteMetadataAndFile(Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "DELETE", REST_PATH, default(KinveyDeleteResponse), urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}
		}





	}
}

