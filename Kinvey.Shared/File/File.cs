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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Kinvey
{
	/// <summary>
	/// This class provides access to Kinvey's File API.
	/// </summary>
	public class File
	{
		#region File class member variables, properties and constructors

		/// <summary>
		/// Gets or sets the Kinvey client.
		/// </summary>
		/// <value>The client.</value>
		private AbstractClient client { get; set; }

		private JObject customRequestProperties = new JObject();

		/// <summary>
		/// Sets a specific custom request property from a Json object.
		/// </summary>
		/// <param name="customheaders">Custom request property as a JObject</param>
		public void SetCustomRequestProperties(JObject customheaders)
		{
			this.customRequestProperties = customheaders;
		}

		/// <summary>
		/// Sets a specific custom request property from a Json object.
		/// </summary>
		/// <param name="key">Custom request property key</param>
		/// <param name="value">Custom request property value as a JObject</param>
		public void SetCustomRequestProperty(string key, JObject value)
		{
			if (this.customRequestProperties == null)
			{
				this.customRequestProperties = new JObject();
			}

			this.customRequestProperties.Add (key, value);
		}

		/// <summary>
		/// Clears the currently saved custom request properties.
		/// </summary>
		public void ClearCustomRequestProperties()
		{
			this.customRequestProperties = new JObject();
		}

		/// <summary>
		/// Gets the custom request properties.
		/// </summary>
		/// <returns>The custom request properties.</returns>
		public JObject GetCustomRequestProperties()
		{
			return this.customRequestProperties;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.File"/> class.
		/// </summary>
		/// <param name="client">[optional] Client (default set to SharedClient).</param>
		public File (AbstractClient client = null)
		{
			if (client != null)
			{
				this.client = client;
			}
			else
			{
				this.client = Client.SharedClient;
			}

			this.customRequestProperties = client.GetCustomRequestProperties ();
			//this.clientAppVersion = client.GetClientAppVersion ();
		}

		#endregion

		#region File class APIs

		#region File class Upload APIs

		/// <summary>
		/// Upload the specified byte[] to Kinvey file storage.  The FileMetaData contains extra data about the file.
		/// </summary>
		/// <param name="metadata">Metadata associated with the file; supports arbitrary key/value pairs.</param>
		/// <param name="content">The actual bytes of the file to upload.</param>
		/// <param name="ct">[optional] The cancellation token.  If cancellation is requested, an OperationCancelledException will be thrown.</param>
		public async Task<FileMetaData> uploadAsync(FileMetaData metadata, byte[] content, CancellationToken ct = default(CancellationToken))
		{
			UploadFileWithMetaDataRequest uploadRequest = buildUploadFileRequest(metadata);
			ct.ThrowIfCancellationRequested();
			FileMetaData fmd = await uploadRequest.ExecuteAsync().ConfigureAwait(false);
			ct.ThrowIfCancellationRequested();
			await uploadRequest.uploadFileAsync(fmd, content).ConfigureAwait(false);
			return fmd;
		}

		/// <summary>
		/// Upload the specified stream to Kinvey file storage.  The FileMetaData contains extra data about the file.
		/// </summary>
		/// <param name="metadata">Metadata associated with the file; supports arbitrary key/value pairs.</param>
		/// <param name="content">The stream of file content to upload.</param>
		/// <param name="ct">[optional] The cancellation token.  If cancellation is requested, an OperationCancelledException will be thrown.</param>
		public async Task<FileMetaData> uploadAsync(FileMetaData metadata, Stream content, CancellationToken ct = default(CancellationToken))
		{
			UploadFileWithMetaDataRequest uploadRequest = buildUploadFileRequest(metadata);
			ct.ThrowIfCancellationRequested();
			FileMetaData fmd = await uploadRequest.ExecuteAsync().ConfigureAwait(false);
			ct.ThrowIfCancellationRequested();
			await uploadRequest.uploadFileAsync(fmd, content).ConfigureAwait(false);
			return fmd;
		}

		/// <summary>
		/// Uploads metadata associated with a file, without changing the file itself.  Do not modify the id or filename using this method-- it's for any other key/value pairs.
		/// </summary>
		/// <param name="metadata">The updated FileMetaData to upload to Kinvey.</param>
		/// <param name="ct">[optional] The cancellation token.  If cancellation is requested, an OperationCancelledException will be thrown.</param>
		public async Task<FileMetaData> uploadMetadataAsync(FileMetaData metadata, CancellationToken ct = default(CancellationToken))
		{
			UploadMetaDataRequest uploadMetaDataRequest = buildUploadMetaDataRequest(metadata);
			ct.ThrowIfCancellationRequested();
			FileMetaData fmd = await uploadMetaDataRequest.ExecuteAsync().ConfigureAwait(false);
			return fmd;
		}

        #endregion

        #region File class download APIs

        [Obsolete("This method has been deprecated (2018-Oct-03).  Please use DownloadAsync() instead.")]
        public async Task<FileMetaData> downloadAsync(FileMetaData metadata, byte[] content, CancellationToken ct = default(CancellationToken))
        {
            return await DownloadAsync(metadata, content, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Download the File associated with the id of the provided metadata.  The file is copied into the byte[], with delegates returning either errors or the FileMetaData from Kinvey.
        /// </summary>
        /// <param name="metadata">The FileMetaData representing the file to download.  This must contain an id.</param>
        /// <param name="content">Content.</param>
        /// <param name="ct">[optional] The cancellation token.  If cancellation is requested, an OperationCancelledException will be thrown.</param>
        public async Task<FileMetaData> DownloadAsync(FileMetaData metadata, byte[] content, CancellationToken ct = default(CancellationToken))
        {
            DownloadFileWithMetaDataRequest downloadRequest = buildDownloadFileRequest(metadata);
			ct.ThrowIfCancellationRequested();
			FileMetaData fmd = await downloadRequest.ExecuteAsync().ConfigureAwait(false);
			ct.ThrowIfCancellationRequested();
			content = await downloadRequest.downloadFileBytesAsync(fmd).ConfigureAwait(false);
			return fmd;
		}

        [Obsolete("This method has been deprecated (2018-Oct-03).  Please use DownloadAsync() instead.")]
        public async Task<FileMetaData> downloadAsync(FileMetaData metadata, Stream content, CancellationToken ct = default(CancellationToken))
        {
            return await DownloadAsync(metadata, content, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Download the File associated with the id of the provided metadata.  The file is streamed into the stream, with delegates returning either errors or the FileMetaData from Kinvey.
        /// </summary>
        /// <param name="metadata">The FileMetaData representing the file to download.  This must contain an id.</param>
        /// <param name="content">Where the contents of the file will be streamed.</param>
        /// <param name="ct">[optional] The cancellation token.  If cancellation is requested, an OperationCancelledException will be thrown.</param>
        public async Task<FileMetaData> DownloadAsync(FileMetaData metadata, Stream content, CancellationToken ct = default(CancellationToken))
		{
			DownloadFileWithMetaDataRequest downloadRequest = buildDownloadFileRequest(metadata);
			ct.ThrowIfCancellationRequested();
			FileMetaData fmd = await downloadRequest.ExecuteAsync().ConfigureAwait(false);
			ct.ThrowIfCancellationRequested();
			await downloadRequest.downloadFileAsync(fmd, content).ConfigureAwait(false);
			return fmd;
		}

		/// <summary>
		/// Downloads the metadata of a File, without actually downloading the file.
		/// </summary>
		/// <param name="fileId">The _id of the file's metadata to download. </param>
		/// <param name="ct">[optional] The cancellation token.  If cancellation is requested, an OperationCancelledException will be thrown.</param>
		public async Task<FileMetaData> downloadMetadataAsync(string fileId, CancellationToken ct = default(CancellationToken))
		{
			DownloadMetaDataRequest downloadMetadataRequest = buildDownloadMetaDataRequest(fileId);
			ct.ThrowIfCancellationRequested();
			FileMetaData fmd = await downloadMetadataRequest.ExecuteAsync().ConfigureAwait(false);
			return fmd;
		}

		#endregion

		#region File class delete APIs

		/// <summary>
		/// Delete the specified file.
		/// </summary>
		/// <param name="fileId">The _id of the file to delete.</param>
		/// <param name="ct">[optional] The cancellation token.  If cancellation is requested, an OperationCancelledException will be thrown.</param>
		public async Task<KinveyDeleteResponse> delete(string fileId, CancellationToken ct = default(CancellationToken))
		{
			DeleteFileAndMetaDataRequest request = buildDeleteFileRequest(fileId);
			ct.ThrowIfCancellationRequested();
			KinveyDeleteResponse deleteResponse = await request.ExecuteAsync().ConfigureAwait(false);
			return deleteResponse;
		}

		#endregion

		#endregion

		#region File class request builder methods

		// Build upload request for file with its corresponding metadata
		private UploadFileWithMetaDataRequest buildUploadFileRequest(FileMetaData metadata)
		{
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			string mode = "POST";
			if (metadata.id != null && metadata.id.Length > 0)
			{
				mode = "PUT";
			}

			// set mimetype for GCS upload
			if (string.IsNullOrEmpty(metadata.mimetype))
			{
				metadata.mimetype = "application/octet-stream";
			}

			UploadFileWithMetaDataRequest uploadRequest = new UploadFileWithMetaDataRequest (metadata, mode, urlParameters, this.client);

			client.InitializeRequest(uploadRequest);
			uploadRequest.customRequestHeaders = this.GetCustomRequestProperties ();

			return uploadRequest;
		}

		// Build upload request for updating metadata for existing file
		private UploadMetaDataRequest buildUploadMetaDataRequest(FileMetaData metadata)
		{
			if (metadata == null ||
				metadata.id == null)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_FILE, EnumErrorCode.ERROR_FILE_UPLOAD_MISSING_METADATA_INFORMATION, "");
			}

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", metadata.id);

			UploadMetaDataRequest uploadMetaDataRequest = new UploadMetaDataRequest(metadata, urlParameters, this.client);

			client.InitializeRequest(uploadMetaDataRequest);
			uploadMetaDataRequest.customRequestHeaders = this.GetCustomRequestProperties();

			return uploadMetaDataRequest;
		}

		// Build download request for file with its corresponding metadata
		private DownloadFileWithMetaDataRequest buildDownloadFileRequest(FileMetaData metadata)
		{
			if (metadata == null ||
				metadata.id == null)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_FILE, EnumErrorCode.ERROR_FILE_DOWNLOAD_MISSING_METADATA_INFORMATION, "");
			}

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", metadata.id);

			DownloadFileWithMetaDataRequest downloadRequest = new DownloadFileWithMetaDataRequest(urlParameters, this.client);

			client.InitializeRequest(downloadRequest);
			//download.clientAppVersion = this.GetClientAppVersion ();
			downloadRequest.customRequestHeaders = this.GetCustomRequestProperties();

			//TODO need more elegant approach for mime type
//			RestSharp.HttpHeader mime = new RestSharp.HttpHeader ();
//			mime.Name  = "x-kinvey-content-type";
//			mime.Value = "application/octet-stream";
//			download.RequestHeaders.Add (mime);

			return downloadRequest;
		}

		// Build download request for the metadata of a specific file
		private DownloadMetaDataRequest buildDownloadMetaDataRequest(String fileId)
		{
			if (fileId == null)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_FILE, EnumErrorCode.ERROR_FILE_MISSING_FILE_ID, "");
			}

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", fileId);

			DownloadMetaDataRequest downloadMetaDataRequest = new DownloadMetaDataRequest(urlParameters, this.client);

			client.InitializeRequest(downloadMetaDataRequest);
			downloadMetaDataRequest.customRequestHeaders = this.GetCustomRequestProperties ();

			return downloadMetaDataRequest;
		}

		// Build delete request for file with its corresponding metadata
		private DeleteFileAndMetaDataRequest buildDeleteFileRequest(String fileId)
		{
			if (fileId == null)
			{
				throw new KinveyException(EnumErrorCategory.ERROR_FILE, EnumErrorCode.ERROR_FILE_MISSING_FILE_ID, "");
			}

			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add ("fileID", fileId);

			DeleteFileAndMetaDataRequest deleteRequest = new DeleteFileAndMetaDataRequest (urlParameters, this.client);

			client.InitializeRequest(deleteRequest);
			deleteRequest.customRequestHeaders = this.GetCustomRequestProperties();

			return deleteRequest;
		}

		#endregion

		#region File class Request inner classes

		// Request to upload file with its corresponding metadata.
		// Used both when uploading file for first time and when updating file.
		[JsonObject(MemberSerialization.OptIn)]
		internal class UploadFileWithMetaDataRequest : KinveyFileRequest
		{
			private const string REST_PATH = "blob/{appKey}/?tls=true";

			[JsonProperty]
			private string fileID { get; set; }

			internal UploadFileWithMetaDataRequest(FileMetaData meta, string mode, Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, mode, REST_PATH, meta, urlProperties)
			{
				if (mode.Equals("PUT"))
				{
					this.fileID = urlProperties["fileID"];
					this.uriTemplate += "/{fileID}";
				}
			}
		}

		// Request to update metadata.
		[JsonObject(MemberSerialization.OptIn)]
		internal class UploadMetaDataRequest : KinveyFileRequest
		{
			private const string REST_PATH = "blob/{appKey}/{fileID}/?tls=true";

			[JsonProperty]
			private string fileID { get; set; }

			internal UploadMetaDataRequest(FileMetaData meta, Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "PUT", REST_PATH, meta, urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}
		}

		// Request to download file with its corresponding metadata.
		[JsonObject(MemberSerialization.OptIn)]
		internal class DownloadFileWithMetaDataRequest : KinveyFileRequest
		{
			private const string REST_PATH = "blob/{appKey}/{fileID}/?tls=true";

			[JsonProperty]
			private string fileID { get; set; }

			internal DownloadFileWithMetaDataRequest(Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "GET", REST_PATH, default(FileMetaData), urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}
		}

		// Request to download metadata.
		[JsonObject(MemberSerialization.OptIn)]
		internal class DownloadMetaDataRequest : KinveyFileRequest
		{
			private const string REST_PATH = "blob/{appKey}/{fileID}/?tls=true";

			[JsonProperty]
			private string fileID { get; set; }

			internal DownloadMetaDataRequest(Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "GET", REST_PATH, default(FileMetaData), urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}
		}

		/// <summary>
		/// A synchronously request to delete metadata and file.
		/// </summary>
		[JsonObject(MemberSerialization.OptIn)]
		internal class DeleteFileAndMetaDataRequest : AbstractKinveyClientRequest<KinveyDeleteResponse>
		{
			private const string REST_PATH = "blob/{appKey}/{fileID}/?tls=true";

			[JsonProperty]
			private string fileID { get; set; }

			internal DeleteFileAndMetaDataRequest(Dictionary<string, string> urlProperties, AbstractClient client)
				: base(client, "DELETE", REST_PATH, default(KinveyDeleteResponse), urlProperties)
			{
				this.fileID = urlProperties["fileID"];
			}
		}

		#endregion
	}
}

