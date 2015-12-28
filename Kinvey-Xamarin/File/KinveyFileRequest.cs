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
using System.Threading.Tasks;
using System.Collections.Generic;
using RestSharp;
using System.IO;
using System.Net.Http;
using ModernHttpClient;

namespace KinveyXamarin
{
	public class KinveyFileRequest : AbstractKinveyClientRequest<FileMetaData>
	{

		public KinveyFileRequest (AbstractClient client, string requestMethod, string uriTemplate, FileMetaData httpContent, Dictionary<string, string> uriParameters)
			: base(client, requestMethod, uriTemplate, httpContent, uriParameters)
		{
		}


		public FileMetaData executeAndDownloadTo (byte[] output){
			FileMetaData metadata = base.Execute ();
			downloadFile (metadata, output);
			return metadata;
		}
			
		public FileMetaData executeAndDownloadTo(ref Stream stream){
			FileMetaData metadata = base.Execute ();
			downloadFile (metadata, ref stream);
			return metadata;
		}

		public FileMetaData executeAndUploadFrom(byte[] input){
			FileMetaData metadata = base.Execute ();
			uploadFile (metadata, input);
			return metadata;
		}

		public FileMetaData executeAndUploadFrom(Stream stream){
			FileMetaData metadata = base.Execute ();
			uploadFile (metadata, stream);
			return metadata;
		}
			
		private void downloadFile(FileMetaData metadata, ref Stream stream){
			string downloadURL = metadata.downloadURL;

			RestClient client = new RestClient (downloadURL);
			RestRequest request = new RestRequest ();

			request.Method = Method.GET;

			var req = client.DownloadDataAsync (request);
			var response = req.Result;

			stream = new MemoryStream (response);

		}

		private void downloadFile(FileMetaData metadata, byte[] output){
			string downloadURL = metadata.downloadURL;

			RestClient client = new RestClient (downloadURL);
			RestRequest request = new RestRequest ();
			request.Method = Method.GET;

			var req = client.ExecuteAsync(request);
			var response = req.Result;

			output = response.RawBytes;
		}

		internal void uploadFile(FileMetaData metadata, byte[] input) {
			uploadFileAsync (metadata, new ByteArrayContent (input)).Wait ();
		}

		internal async Task uploadFileAsync(FileMetaData metadata, byte[] input) {
			await uploadFileAsync (metadata, new ByteArrayContent (input));
		}

		internal void uploadFile(FileMetaData metadata, Stream input) {
			uploadFileAsync (metadata, new StreamContent (input)).Wait ();
		}

		internal async Task uploadFileAsync(FileMetaData metadata, Stream input) {
			if (input.CanSeek) {
				input.Position = 0;
			}
			await uploadFileAsync (metadata, new StreamContent (input));
		}

		private void uploadFile(FileMetaData metadata, HttpContent input) {
			uploadFileAsync (metadata, input).Wait ();
		}

		private async Task<HttpResponseMessage> uploadFileAsync(FileMetaData metadata, HttpContent input) {
			string uploadURL = metadata.uploadUrl;

			var httpClient = new HttpClient(new NativeMessageHandler());
			Uri requestURI = new Uri (uploadURL);

			foreach (var header in metadata.headers) {
				httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
			}

			var response = await httpClient.PutAsync(requestURI, input);
			response.EnsureSuccessStatusCode ();
			return response;
		}
	}
}

