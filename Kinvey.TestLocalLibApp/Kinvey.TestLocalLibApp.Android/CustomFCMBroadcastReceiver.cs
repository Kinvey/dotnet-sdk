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

namespace Kinvey.TestLocalLibApp.Droid
{
    [BroadcastReceiver(Permission = "com.google.android.c2dm.permission.SEND")]
    //[IntentFilter(new string[] { "com.google.android.c2dm.intent.RECEIVE" })]
    //[IntentFilter(new string[] { "com.google.android.c2dm.intent.REGISTRATION" })]
    //[IntentFilter(new string[] { "com.kinvey.xamarin.android.fcm.unregistration" })]
    //[IntentFilter(new string[] { "com.google.android.gcm.intent.RETRY" })]
    //[IntentFilter(new string[] { "com.kinvey.xamarin.android.ERROR" })]
    public class CustomFCMBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Bundle bundle = intent.Extras;
            Dictionary<string, object> dict = bundle.KeySet()
                .ToDictionary<string, string, object>(key => key, key => bundle.Get(key));

            Intent i = new Intent(context, typeof(FCMService));
            i.SetAction(intent.Action);
            i.PutExtras(intent.Extras);
            context.StartService(i);
            SetResult(Result.Ok, null, null);
        }
    }
}