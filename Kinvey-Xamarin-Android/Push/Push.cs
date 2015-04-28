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
	public class Push : AbstractPush
	{
		
		private const string GCM_ID = "gcmid";

		public Push (Client client) : base(client){}

		public void Initialize(Context appContext){

			string senders = base.client.senderID;

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
					EnablePushViaRest ("android", gcmID).Execute();

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
				DisablePushViaRest("android", alreadyInitialized).Execute();
			});
		}
	}
}

