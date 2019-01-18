using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;

namespace Kinvey.TestLocalLibApp.Droid
{
    [Activity(Label = "Kinvey.TestLocalLibApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private CustomFCMBroadcastReceiver _customFCMBroadcastReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            _customFCMBroadcastReceiver = new CustomFCMBroadcastReceiver();
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());          
        }

        protected override void OnResume()
        {
            base.OnResume();
            var intentFilter = new IntentFilter();
            intentFilter.AddAction(Constants.STR_C2DM_INTENT_RECEIVE);
            intentFilter.AddAction(Constants.STR_C2DM_INTENT_REGISTRATION);
            intentFilter.AddAction(Constants.STR_KINVEY_FCM_UNREGISTRATION);
            intentFilter.AddAction(Constants.STR_KINVEY_ANDROID_ERROR);

            RegisterReceiver(_customFCMBroadcastReceiver, intentFilter);
        }
        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver(_customFCMBroadcastReceiver);
        }
    }
}
