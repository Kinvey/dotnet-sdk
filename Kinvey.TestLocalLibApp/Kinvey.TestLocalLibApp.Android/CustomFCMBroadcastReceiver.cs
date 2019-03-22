using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;

namespace Kinvey.TestLocalLibApp.Droid
{
    [BroadcastReceiver(Permission = "com.google.android.c2dm.permission.SEND")]
    public class CustomFCMBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {         
            var i = new Intent(context, typeof(FCMService));
            i.SetAction(intent.Action);
            i.PutExtras(intent.Extras);
            context.StartService(i);
            SetResult(Result.Ok, null, null);
        }
    }
}