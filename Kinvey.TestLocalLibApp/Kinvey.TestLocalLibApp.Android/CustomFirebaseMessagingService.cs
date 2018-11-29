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
using Firebase.Messaging;

namespace Kinvey.TestLocalLibApp.Droid
{

    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class CustomFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            var body = message.GetNotification().Body;
        }
    }
}