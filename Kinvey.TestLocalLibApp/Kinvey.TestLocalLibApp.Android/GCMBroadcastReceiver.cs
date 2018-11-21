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
    [IntentFilter(new string[] { "com.google.android.c2dm.intent.RECEIVE" })]
    [IntentFilter(new string[] { "com.google.android.c2dm.intent.REGISTRATION" })]
    [IntentFilter(new string[] { "com.google.android.gcm.intent.RETRY" })]
    [IntentFilter(new string[] { "com.kinvey.xamarin.android.ERROR" })]
    public class GCMBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Intent i = new Intent(context, typeof(GCMService));
            i.SetAction(intent.Action);
            i.PutExtras(intent.Extras);
            context.StartService(i);
            SetResult(Result.Ok, null, null);
        }
    }
}