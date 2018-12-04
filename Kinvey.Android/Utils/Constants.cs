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
    public static partial class Constants
    {
        public const string C2DM_INTENT_REGISTRATION = "com.google.android.c2dm.intent.REGISTRATION";
        public const string KINVEY_FCM_UNREGISTRATION = "com.kinvey.xamarin.android.fcm.unregistration";
        public const string UNREGISTRATION_ID = "unregistration_id";
    }
}