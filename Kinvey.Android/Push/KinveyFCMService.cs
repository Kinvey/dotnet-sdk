using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Kinvey
{
    [Service]
    public abstract class KinveyFCMService : IntentService
    {
        public KinveyFCMService()
        { }

        static PowerManager.WakeLock sWakeLock;
        static object LOCK = new object();
        private const string MESSAGE_FROM_FCM = "msg";
        private const string C2DM_INTENT_REGISTRATION = "com.google.android.c2dm.intent.REGISTRATION";
        private const string REGISTRATION_ID = "registration_id";

        protected override void OnHandleIntent(Intent intent)
        {

            lock (LOCK)
            {
                if (sWakeLock == null)
                {
                    var pm = PowerManager.FromContext(this.ApplicationContext);
                    sWakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "KinveyFCM");
                }
            }

            sWakeLock.Acquire();


            try
            {
                string action = intent.Action;

                if (action.Equals(C2DM_INTENT_REGISTRATION))
                {
                    var registrationId = intent.GetStringExtra(REGISTRATION_ID);
                    onRegistered(registrationId);
                }
                else if (action.Equals(Constants.KINVEY_FCM_UNREGISTRATION))
                {
                    var unregistrationId = intent.GetStringExtra(Constants.UNREGISTRATION_ID);
                    onRegistered(unregistrationId);
                }
                else if (action.Equals("com.google.android.c2dm.intent.RECEIVE"))
                {
                    onMessage(intent.GetStringExtra(MESSAGE_FROM_FCM));
                }
                else if (action.Equals("delete"))
                {
                    onDelete(intent.GetIntExtra("DELETED", 0));
                }
                else if (action.Equals("com.kinvey.xamarin.android.ERROR"))
                {
                    onError(intent.GetStringExtra("ERROR"));
                }
            }
            finally
            {
                lock (LOCK)
                {
                    //Sanity check for null as this is a public method
                    if (sWakeLock != null)
                        sWakeLock.Release();
                }
            }
        }

        public abstract void onMessage(string message);

        public abstract void onError(string error);

        public abstract void onDelete(int deleted);

        public abstract void onRegistered(string registrationId);

        public abstract void onUnregistered(string unregistrationId);

    }
}