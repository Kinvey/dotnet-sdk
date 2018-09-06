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
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using KinveyUtils;

namespace Kinvey
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
            int startPosition = await CheckResumableStateAsync(metadata.uploadUrl, (int)metadata.size);
            await uploadFileAsync(metadata, new ByteArrayContent(input, startPosition, (int)metadata.size -startPosition));
        }

		internal async Task uploadFileAsync(FileMetaData metadata, Stream input)
		{
            if (input.CanSeek)
            {
                int startPosition = await CheckResumableStateAsync(metadata.uploadUrl, (int)metadata.size);
                input.Position = startPosition;
            }

			await uploadFileAsync(metadata, new StreamContent(input));
		}

		private async Task<HttpResponseMessage> uploadFileAsync(FileMetaData metadata, HttpContent input)
		{
			string uploadURL = metadata.uploadUrl;

			MediaTypeHeaderValue mt = new MediaTypeHeaderValue(metadata.mimetype);
			input.Headers.ContentType = mt;

			var httpClient = new HttpClient();
			Uri requestURI = new Uri(uploadURL);

			foreach (var header in metadata.headers)
			{
				httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
			}

			var response = await httpClient.PutAsync(requestURI, input);
			response.EnsureSuccessStatusCode();
			return response;
		}

        internal async Task<int> CheckResumableStateAsync(string uploadURL, int contentSize)
        {
            int startByte = 0;

            // Create empty HTTP PUT request to the GCS URI given from KCS
            var httpClient = new HttpClient();
            Uri requestURI = new Uri(uploadURL);
            var httpRequest = new System.Net.Http.HttpRequestMessage(HttpMethod.Put, requestURI);
            var content = new StringContent("");
            content.Headers.ContentLength = 0;
            content.Headers.ContentRange = new ContentRangeHeaderValue(contentSize);
            httpRequest.Content = content;
            try
            {
                Logger.Log(httpRequest);
                var httpResponse = await httpClient.SendAsync(httpRequest);
                Logger.Log(httpResponse);
            }
            catch (System.Net.Http.HttpRequestException hre)
            {
                var innerEx = hre.InnerException as System.Net.WebException;
                var actualResponse = innerEx.Response;
                var status = innerEx.Status;
                string innermsg = innerEx.Message;

                if (actualResponse == null && string.Compare("Invalid status code: 308", innermsg) == 0)
                {
                    // This is the Xamarin/Mono case, where the inner exception
                    // does not give back the 308 response.  In this case, all
                    // we can do for now is attempt the whole upload.
                    // Do nothing for now.
                }
                else
                {
                    try
                    {
                        // Check response for status code
                        var resp = actualResponse as System.Net.HttpWebResponse;
                        switch ((int)resp.StatusCode)
                        {
                            case 200:
                            case 201:
                                // Already uploaded - no need to attempt upload
                                break;

                            case 308:
                                // Resumable file upload case - check range header
                                var rangeHeader = resp.Headers[System.Net.HttpRequestHeader.Range];
                                startByte = DetermineStartByteFromRange(rangeHeader);

                                break;

                            default:
                                // Attempt full upload
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // Something went wrong in parsing the Range header, so
                        // attempt the whole upload.
                    }
                }
            }

            return startByte;
        }

        static internal int DetermineStartByteFromRange(string rangeHeaderValue)
        {
            int startByte = 0;

            if (rangeHeaderValue != null)
            {
                // Parse Range header and set the start byte accordingly
                int lastByteSent = 0;

                // Example format: bytes=0-42
                char[] delims = new char[] { '-' };
                lastByteSent = Int32.Parse(rangeHeaderValue.Split(delims)[1]);
                startByte = lastByteSent + 1;
            }

            return startByte;
        }

        #endregion

        #region KinveyFileRequest download methods

		internal async Task downloadFileAsync(FileMetaData metadata, Stream stream)
		{
			var response = await downloadFileAsync(metadata);
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                await responseStream.CopyToAsync(stream);
            }
		}

		internal async Task<byte[]> downloadFileBytesAsync(FileMetaData metadata)
		{
			var response = await downloadFileAsync(metadata);
            var output = await response.Content.ReadAsByteArrayAsync();
            return output;
		}

		private async Task<HttpResponseMessage> downloadFileAsync(FileMetaData metadata)
		{
			string downloadURL = metadata.downloadURL;
			var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, downloadURL);
            Logger.Log(request);
            var response = await client.SendAsync(request);
            Logger.Log(response);
            return response;
		}

        #endregion
	}
}

