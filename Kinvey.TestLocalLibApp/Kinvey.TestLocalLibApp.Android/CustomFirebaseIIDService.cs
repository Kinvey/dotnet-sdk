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
using Firebase.Iid;
using Firebase.Messaging;

namespace Kinvey.TestLocalLibApp.Droid
{
    //[Service]
    //[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    //public class CustomFirebaseIIDService : FirebaseInstanceIdService
    //{
    //    public override void OnTokenRefresh()
    //    {
    //        var refreshedToken = FirebaseInstanceId.Instance.Token;
    //        FirebaseMessaging.Instance.SubscribeToTopic("news");
    //    }
    //}
}