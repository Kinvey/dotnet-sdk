using System;
using KinveyXamarin;
using KinveyUtils;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Android.Content;
using Android.App;
using Android.Gms.Gcm;
using System.Threading;

namespace KinveyXamarinAndroid
{
	public class Push
	{
		private Client client;


		public Push (Client client)
		{
			this.client = client;
		}


		public async void Initialize(Context appContext){

			string senders = this.client.senderID;

			ThreadPool.QueueUserWorkItem(o =>
				{
					var gcm = GoogleCloudMessaging.GetInstance(appContext);
					var gcmID = gcm.Register(senders);  
					//
					Logger.Log ("-------sender ID is: " + senders);
					Logger.Log ("-------GCM ID is: " + gcmID);
					//
					PushPayload response =  EnablePushViaRest (gcmID).Execute();
					//
					//

					Intent intent = new Intent("com.google.android.c2dm.intent.REGISTER");
					intent.SetPackage("com.google.android.gsf");
					intent.PutExtra("app", PendingIntent.GetBroadcast(appContext, 0, new Intent(), 0));
					intent.PutExtra("sender", senders);
					//intent.PutExtra ("gcmID", gcmID);
					appContext.StartService(intent);
				});

//			var gcm = GoogleCloudMessaging.GetInstance(appContext);
//			string gcmID = gcm.Register (senders);
//
//			Logger.Log ("-------sender ID is: " + senders);
//			Logger.Log ("-------GCM ID is: " + gcmID);
////
//			PushPayload response = await EnablePushViaRest (gcmID).ExecuteAsync();
////
////
//
//			Intent intent = new Intent("com.google.android.c2dm.intent.REGISTER");
//			intent.SetPackage("com.google.android.gsf");
//			intent.PutExtra("app", PendingIntent.GetBroadcast(appContext, 0, new Intent(), 0));
//			intent.PutExtra("sender", senders);
//			//intent.PutExtra ("gcmID", gcmID);
//			appContext.StartService(intent);
		}

		public EnablePush EnablePushViaRest(string deviceId){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload (deviceId);


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

