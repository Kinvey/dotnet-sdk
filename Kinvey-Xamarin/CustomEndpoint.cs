using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KinveyXamarin
{
	public class CustomEndpoint<I, O>
	{

		private AbstractClient client;


		public CustomEndpoint (AbstractClient client)
		{
			this.client = client;
		}

		public CustomCommand executeCustomEndpointBlocking(string endpoint, I input){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("endpoint", endpoint);

			CustomCommand custom = new CustomCommand (client, endpoint, input, urlParameters);

			client.InitializeRequest(custom);

			return custom;
		}

		public CustomCommandArray executeCustomEndpointArrayBlocking(string endpoint, I input){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);
			urlParameters.Add("endpoint", endpoint);

			CustomCommandArray custom = new CustomCommandArray (client, endpoint, input, urlParameters);

			client.InitializeRequest(custom);

			return custom;
		}



		/// <summary>
		/// Executes a custom endpoint expecting a single result
		/// </summary>
		public class CustomCommand : AbstractKinveyClientRequest<O> {
			private const string REST_PATH = "rpc/{appKey}/custom/{endpoint}";

			[JsonProperty]
			public string endpoint;

			public CustomCommand(AbstractKinveyClient client, string endpoint, I input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){
				this.endpoint = endpoint;
			}
		}


		/// <summary>
		/// Executes a custom endpoint expecting an array of results
		/// </summary>
		public class CustomCommandArray : AbstractKinveyClientRequest<O[]> {
			private const string REST_PATH = "rpc/{appKey}/custom/{endpoint}";

			[JsonProperty]
			public string endpoint;

			public CustomCommandArray(AbstractKinveyClient client, string endpoint, I input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){
				this.endpoint = endpoint;
			}
		}





	}
}

