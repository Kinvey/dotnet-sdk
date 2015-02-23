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
using Android.Preferences;

namespace KinveyXamarinAndroid
{
	public class Push
	{
		private Client client;
		private const string GCM_ID = "gcmid";


		public Push (Client client)
		{
			this.client = client;
		}


		public void Initialize(Context appContext){

			string senders = this.client.senderID;

			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (appContext);
			string alreadyInitialized = prefs.GetString (GCM_ID, "");

			if (alreadyInitialized.Length > 0) {
				//this device has already registered for push
				return;
			}

			ThreadPool.QueueUserWorkItem(o => {

				Intent intent;

				try{
					var gcm = GoogleCloudMessaging.GetInstance(appContext);
					var gcmID = gcm.Register(senders); 

					Logger.Log ("-------sender ID is: " + senders);
					Logger.Log ("-------GCM ID is: " + gcmID);

					//Response contains No Content
					EnablePushViaRest (gcmID).Execute();

					ISharedPreferencesEditor editor = prefs.Edit ();
					editor.PutString (GCM_ID, gcmID);
					editor.Apply();  

					intent = new Intent("com.google.android.c2dm.intent.REGISTER");
					intent.SetPackage("com.google.android.gsf");
					intent.PutExtra("app", PendingIntent.GetBroadcast(appContext, 0, new Intent(), 0));
					intent.PutExtra("sender", senders);
					appContext.StartService(intent);
				}catch(Exception e){
					intent = new Intent("com.kinvey.xamarin.android.ERROR");
					intent.PutExtra("ERROR", e.Message);
					appContext.SendBroadcast(intent);
				}
					

			});
		}


		public void DisablePush(Context appContext){
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (appContext);
			string alreadyInitialized = prefs.GetString (GCM_ID, "");

			if (alreadyInitialized.Length == 0) {
				//this device has not already registered for push
				return;
			}

			ThreadPool.QueueUserWorkItem (o => {
				DisablePushViaRest(alreadyInitialized).Execute();
			});


		}

		public EnablePush EnablePushViaRest(string deviceId){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload (deviceId);


			EnablePush enable = new EnablePush (client, input, urlParameters);

			client.InitializeRequest(enable);

			return enable;
		}

		public RemovePush DisablePushViaRest(string deviceId){
			var urlParameters = new Dictionary<string, string>();
			urlParameters.Add("appKey", ((KinveyClientRequestInitializer)client.RequestInitializer).AppKey);

			PushPayload input = new PushPayload ();

			RemovePush disable = new RemovePush (client, input, urlParameters);

			client.InitializeRequest(disable);

			return disable;
		}


		public class EnablePush : AbstractKinveyClientRequest<PushPayload> {
			private const string REST_PATH = "push/{appKey}/register-device";

			public EnablePush(AbstractKinveyClient client, PushPayload input, Dictionary<string, string> urlProperties) :
			base(client, "POST", REST_PATH, input, urlProperties){

			}
		}

		public class RemovePush : AbstractKinveyClientRequest<PushPayload> {
			private const string REST_PATH = "push/{appKey}/unregister-device";

			public RemovePush(AbstractKinveyClient client, PushPayload input, Dictionary<string, string> urlProperties) :
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

