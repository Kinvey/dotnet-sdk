using System;
using System.Collections.Generic;
using RestSharp;

namespace KinveyXamarin
{
	public class KinveyFileRequest : AbstractKinveyClientRequest<FileMetaData>
	{

		public KinveyFileRequest (AbstractKinveyClient client, string requestMethod, string uriTemplate, FileMetaData httpContent, Dictionary<string, string> uriParameters)
			: base(client, requestMethod, uriTemplate, httpContent, uriParameters)
		{
		}


		public void executeAndDownloadTo (byte[] output){

			FileMetaData metadata = base.Execute ();

			downloadFile (metadata, output);

		}

		public void executeAndUploadFrom(byte[] input){

			FileMetaData metadata = base.Execute ();

			uploadFile (metadata, input);

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

		private void uploadFile(FileMetaData metadata, byte[] input){
			string uploadURL = metadata.uploadUrl;

			var client = new RestClient (uploadURL);

			var request = new RestRequest ();

			if (requestMethod.Equals ("PUT")) {
				request.Method = Method.PUT;
			} else {
				request.Method = Method.POST;
			}

			//TODO what are these parameters for `name` and `filename` used for?
			request.AddFile ("test", input, "filenameTest");

			var req = client.ExecuteAsync (request);
			var response = req.Result;
		}



	}
}

