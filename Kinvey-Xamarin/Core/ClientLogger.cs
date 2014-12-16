using System;
using RestSharp;

namespace KinveyXamarin
{
	public class ClientLogger
	{

		public static bool initialized {get; set;} = false;

		public static Client client { get; set;}

		public static void initialize(Client client){
			ClientLogger.client = client;
			ClientLogger.initialized = true;
		}

	
		public static void Log(String message){
			if (!initialized) {
				return;
			}
			client.logger (message);
		}


		public static void Log(Exception e){
			if (!initialized) {
				return;
			}
			client.logger (e.ToString());
		}

		public static void Log(RestRequest request){
			if (!initialized) {
				return;
			}
			client.logger ("------------------------REQUEST");
			client.logger (request.Method + " -> " + request.Resource);
			client.logger ("------------------------END REQUEST");


		}

		public static void Log(IRestResponse response){
			if (!initialized) {
				return;
			}

			client.logger ("------------------------RESPONSE");
			client.logger (response.ToString ());
			client.logger ("------------------------END RESPONSE");


		}

	}
}
