using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Iid;

namespace Kinvey
{
    public class FCMPush : AbstractPush
    {
        private const string FCM_ID = "fcmid";
        private const string C2DM_INTENT_REGISTER = "com.google.android.c2dm.intent.REGISTER";
        private const string ANDROID_GSF = "com.google.android.gsf";
        private const string APP = "app";
        private const string SENDER = "sender";
        private const string ANDROID = "android";
        private const string FCM_NOT_REGISTERED = "Firebase Cloud Messaging is not registered";


        public FCMPush(Client client) : base(client) { }

        public void Initialize(Context appContext)
        {
            var senders = base.client.senderID;

            ThreadPool.QueueUserWorkItem(o => {

                Intent intent;

                try
                {
                    var token = FirebaseInstanceId.Instance.Token;

                    if (string.IsNullOrEmpty(token))
                    {
                        intent = new Intent(Constants.STR_KINVEY_ANDROID_ERROR);
                        intent.PutExtra(Constants.STR_GENERAL_ERROR, FCM_NOT_REGISTERED);
                        appContext.SendBroadcast(intent);
                        return;
                    }

                    //EnablePushViaRest(ANDROID, token).Execute();

                    var prefs = PreferenceManager.GetDefaultSharedPreferences(appContext);
                    ISharedPreferencesEditor editor = prefs.Edit();
                    editor.PutString(FCM_ID, token);
                    editor.Apply();

                    intent = new Intent(C2DM_INTENT_REGISTER);
                    intent.SetPackage(ANDROID_GSF);
                    intent.PutExtra(APP, PendingIntent.GetBroadcast(appContext, 0, new Intent(), 0));
                    intent.PutExtra(SENDER, senders);
                    appContext.StartService(intent);
                }
                catch (Exception ex)
                {
                    intent = new Intent(Constants.STR_KINVEY_ANDROID_ERROR);
                    intent.PutExtra(Constants.STR_GENERAL_ERROR, ex.Message);
                    appContext.SendBroadcast(intent);
                }
            });
        }


        public void DisablePush(Context appContext)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(appContext);
            var alreadyInitialized = prefs.GetString(FCM_ID, string.Empty);

            if (alreadyInitialized.Length == 0)
            {
                //this device has not already registered for push
                return;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                Intent intent;

                try
                {
                    //DisablePushViaRest(ANDROID, alreadyInitialized).Execute();

                    ISharedPreferencesEditor editor = prefs.Edit();
                    editor.Remove(FCM_ID);
                    editor.Apply();

                    intent = new Intent(Constants.STR_KINVEY_FCM_UNREGISTRATION);
                    intent.PutExtra(Constants.STR_UNREGISTRATION_ID, alreadyInitialized);
                    appContext.SendBroadcast(intent);
                }
                catch (Exception ex)
                {
                    intent = new Intent(Constants.STR_KINVEY_ANDROID_ERROR);
                    intent.PutExtra(Constants.STR_GENERAL_ERROR, ex.Message);
                    appContext.SendBroadcast(intent);
                }
            });
        }
    }
}