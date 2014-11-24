using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace KinveyXamarin
{
	public class File
	{
		private AbstractClient client {get; set;}

		public FileProgressListener progressListener { get; set;}


		public File (AbstractClient client)
		{
			this.client = client;
		}

		public DownloadMetadataAndFile downloadBlocking(FileMetaData metadata){

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", KAssert.notNull(metadata.id, "metadata.id is required to download a specific file-- can also download by query if _id is unknown."));

			DownloadMetadataAndFile download = new DownloadMetadataAndFile (metadata, progressListener, urlParameters, this.client);

			client.InitializeRequest (download);


			//TODO refactor this
//			RestSharp.HttpHeader mime = new RestSharp.HttpHeader ();
//			mime.Name  = "x-kinvey-content-type";
//			mime.Value = "application/octet-stream";
//			download.RequestHeaders.Add (mime);

			return download;
		}

		public UploadMetadataAndFile uploadBlocking(FileMetaData metadata){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			//urlParameters.Add ("fileID", KAssert.notNull(metadata.id, "metadata.id is required to download a specific file-- can also download by query if _id is unknown."));

			string mode = "POST";
			if (metadata.id != null && metadata.id.Length > 0) {
				mode = "PUT";
			}

			UploadMetadataAndFile upload = new UploadMetadataAndFile (metadata, mode, progressListener, urlParameters, this.client);

			client.InitializeRequest (upload);
		
			return upload;
		}



		[JsonObject(MemberSerialization.OptIn)]
		public class DownloadMetadataAndFile : KinveyFileRequest
		{

			private const string REST_PATH = "blob/{appkey}/{fileID}";

			[JsonProperty]
			public string fileID { get; set;}

			public DownloadMetadataAndFile(FileMetaData meta, FileProgressListener progressListener, Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "GET", REST_PATH, default(FileMetaData), urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}
				

		}

		[JsonObject(MemberSerialization.OptIn)]
		public class UploadMetadataAndFile : KinveyFileRequest
		{

			private const string REST_PATH = "blob/{appkey}";

			[JsonProperty]
			public string fileID { get; set;}

			public UploadMetadataAndFile(FileMetaData meta, string mode, FileProgressListener progressListener, Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, mode, REST_PATH, default(FileMetaData), urlProperties)
			{

				if (mode.Equals("PUT"))
				{
					this.fileID = urlProperties["fileID"];
					this.uriTemplate += "/{fileID}";
				}
			}
		}
	}
}

