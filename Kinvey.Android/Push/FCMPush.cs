using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Preferences;
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
        private const string FCM_NOT_REGISTERED = "Firebase Cloud Messaging is not registered";
        private const string FirebaseInstanceIdInternalReceiver = "com.google.firebase.iid.FirebaseInstanceIdInternalReceiver";
        private const string FirebaseInstanceIdReceiver = "com.google.firebase.iid.FirebaseInstanceIdReceiver";

        public FCMPush(Client client) : base(client) { }

        public async Task InitializeAsync(Context appContext)
        {
            CheckPushReceiversExistence(appContext);
            CheckKinveyFCMServiceClassOverrideExistence();

            var senders = base.client.senderID;
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

                await EnablePushAsync(ANDROID, token).ConfigureAwait(false);

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
        }


        public async Task DisablePushAsync(Context appContext)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(appContext);
            var alreadyInitialized = prefs.GetString(FCM_ID, string.Empty);

            if (alreadyInitialized.Length == 0)
            {
                //this device has not already registered for push
                return;
            }

            Intent intent;

            try
            {
                await DisablePushAsync(ANDROID, alreadyInitialized).ConfigureAwait(false);
                await DeleteFirebaseInstanceId().ConfigureAwait(false);

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
        }

        private void CheckPushReceiversExistence(Context appContext)
        {
            var packageInfo = appContext.PackageManager.GetPackageInfo(appContext.PackageName, PackageInfoFlags.Receivers);

            var existFirebaseInstanceIdInternalReceiver = packageInfo.Receivers.Any(receiver => receiver.Name.Equals(FirebaseInstanceIdInternalReceiver));
            var existFirebaseInstanceIdReceiver = packageInfo.Receivers.Any(receiver => receiver.Name.Equals(FirebaseInstanceIdReceiver));

            if (!existFirebaseInstanceIdInternalReceiver || !existFirebaseInstanceIdReceiver)
            {
                throw new KinveyException(EnumErrorCategory.ERROR_REQUIREMENT, EnumErrorCode.ERROR_REQUIREMENT_MISSING_PUSH_CONFIGURATION_RECEIVERS, string.Empty);
            }
        }

        private async Task DeleteFirebaseInstanceId()
        {
            await Task.Run(() => {
                FirebaseInstanceId.Instance.DeleteInstanceId();
                }).ConfigureAwait(false);
        }

        private void CheckKinveyFCMServiceClassOverrideExistence()
        {
            Type kinveyFCMServiceSubType = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                kinveyFCMServiceSubType = assembly.GetTypes().FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(KinveyFCMService)));

                if (kinveyFCMServiceSubType != null)
                {
                    break;
                }
            }

            if (kinveyFCMServiceSubType == null)
            {
                throw new KinveyException(EnumErrorCategory.ERROR_REQUIREMENT, EnumErrorCode.ERROR_REQUIREMENT_MISSING_PUSH_CONFIGURATION_CLASS_OVERRIDE, string.Empty);
            }
        }

    }
}