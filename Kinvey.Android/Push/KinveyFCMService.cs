using Android.App;
using Android.Content;
using Android.OS;

namespace Kinvey
{
    [Service]
    public abstract class KinveyFCMService : IntentService
    {
        public KinveyFCMService()
        { }

        static PowerManager.WakeLock sWakeLock;
        static object LOCK = new object();
        private const string MESSAGE_FROM_FCM = "gcm.notification.body";
        private const string REGISTRATION_ID = "registration_id";
        private const string KINVEY_FCM = "KinveyFCM";

        protected override void OnHandleIntent(Intent intent)
        {

            lock (LOCK)
            {
                if (sWakeLock == null)
                {
                    var pm = PowerManager.FromContext(this.ApplicationContext);
                    sWakeLock = pm.NewWakeLock(WakeLockFlags.Partial, KINVEY_FCM);
                }
            }

            sWakeLock.Acquire();


            try
            {
                string action = intent.Action;

                if (action.Equals(Constants.STR_C2DM_INTENT_REGISTRATION))
                {
                    var registrationId = intent.GetStringExtra(REGISTRATION_ID);
                    onRegistered(registrationId);
                }
                else if (action.Equals(Constants.STR_KINVEY_FCM_UNREGISTRATION))
                {
                    var unregistrationId = intent.GetStringExtra(Constants.STR_UNREGISTRATION_ID);
                    onUnregistered(unregistrationId);
                }
                else if (action.Equals(Constants.STR_C2DM_INTENT_RECEIVE))
                {
                    onMessage(intent.GetStringExtra(MESSAGE_FROM_FCM));
                }
                else if (action.Equals(Constants.STR_KINVEY_ANDROID_ERROR))
                {
                    onError(intent.GetStringExtra(Constants.STR_GENERAL_ERROR));
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

        public abstract void onRegistered(string registrationId);

        public abstract void onUnregistered(string unregistrationId);

    }
}