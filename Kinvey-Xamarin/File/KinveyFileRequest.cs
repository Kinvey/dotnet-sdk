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
using System.Net.Http.Headers;
using ModernHttpClient;

namespace KinveyXamarin
{
	internal class KinveyFileRequest : AbstractKinveyClientRequest<FileMetaData>
	{
		internal KinveyFileRequest (AbstractClient client, string requestMethod, string uriTemplate, FileMetaData httpContent, Dictionary<string, string> uriParameters)
			: base(client, requestMethod, uriTemplate, httpContent, uriParameters)
		{
		}

		#region KinveyFileRequest upload methods

		internal async Task uploadFileAsync(FileMetaData metadata, byte[] input)
		{
			await uploadFileAsync (metadata, new ByteArrayContent (input));
		}

		internal async Task uploadFileAsync(FileMetaData metadata, Stream input)
		{
			if (input.CanSeek)
			{
				input.Position = 0;
			}

			await uploadFileAsync(metadata, new StreamContent(input));
		}

		private async Task<HttpResponseMessage> uploadFileAsync(FileMetaData metadata, HttpContent input)
		{
			string uploadURL = metadata.uploadUrl;

			MediaTypeHeaderValue mt = new MediaTypeHeaderValue(metadata.mimetype);
			input.Headers.ContentType = mt;

			var httpClient = new HttpClient(new NativeMessageHandler());
			Uri requestURI = new Uri(uploadURL);

			foreach (var header in metadata.headers)
			{
				httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
			}

			var response = await httpClient.PutAsync(requestURI, input);
			response.EnsureSuccessStatusCode();
			return response;
		}

		#endregion

		#region KinveyFileRequest download methods

		internal async Task downloadFileAsync(FileMetaData metadata, Stream stream)
		{
			IRestResponse response = await downloadFileAsync(metadata);
			MemoryStream ms = new MemoryStream(response.RawBytes);
			ms.CopyTo(stream);
		}

		internal async Task downloadFileAsync(FileMetaData metadata, byte[] output)
		{
			IRestResponse response = await downloadFileAsync(metadata);
			output = response.RawBytes;
		}

		private async Task<IRestResponse> downloadFileAsync(FileMetaData metadata)
		{
			string downloadURL = metadata.downloadURL;
			RestClient client = new RestClient(downloadURL);

			RestRequest request = new RestRequest();
			request.Method = Method.GET;

			IRestResponse response = await client.ExecuteAsync(request);
			return response;
		}

		#endregion
	}
}

