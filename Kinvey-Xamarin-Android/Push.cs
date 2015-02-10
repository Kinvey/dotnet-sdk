using System;
using KinveyXamarin;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace KinveyXamarinAndroid
{
	public class Push
	{
		private Client client;


		public Push (Client client)
		{
			this.client = client;
		}

		public EnablePush EnablePushViaRest(string deviceId){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload ();

			EnablePush enable = new EnablePush (client, input, urlParameters);

			client.InitializeRequest(enable);

			return enable;
		}

		public DisablePush DisablePushViaRest(string deviceId){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload ();

			DisablePush disable = new DisablePush (client, input, urlParameters);

			client.InitializeRequest(disable);

			return disable;
		}


		public class EnablePush : AbstractKinveyClientRequest<PushPayload> {
			private const string REST_PATH = "push/{appKey}/register-device";

			public EnablePush(AbstractKinveyClient client, PushPayload input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){

			}
		}

		public class DisablePush : AbstractKinveyClientRequest<PushPayload> {
			private const string REST_PATH = "push/{appKey}/unregister-device";

			public DisablePush(AbstractKinveyClient client, PushPayload input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){

			}
		}


		[JsonObject(MemberSerialization.OptIn)]
		public class PushPayload : JObject{

			[JsonProperty]
			private String platform { get; set;} 

			[JsonProperty]
			private String deviceId {get; set;}

			public PushPayload() {
				this.platform = "android";
			}

			public PushPayload(String deviceId) {
				this.platform = "android";
				this.deviceId = deviceId;
			}


		}


	}
}

