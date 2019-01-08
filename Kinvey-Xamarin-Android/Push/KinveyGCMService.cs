using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Gms.Gcm;

namespace Kinvey
{
	[Service]
	public abstract class KinveyGCMService : IntentService
	{
		public KinveyGCMService ()
		{}

		static PowerManager.WakeLock sWakeLock;
		static object LOCK = new object();
		private const string MESSAGE_FROM_GCM = "msg";

		protected override void OnHandleIntent(Intent intent)
		{

			lock (LOCK)
			{
				if (sWakeLock == null)
				{
					var pm = PowerManager.FromContext(this.ApplicationContext);
					sWakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "KinveyGCM");
				}
			}

			sWakeLock.Acquire();


			try
			{
				string action = intent.Action;

				if (action.Equals("com.google.android.c2dm.intent.REGISTRATION"))
				{
					string gcmID = intent.GetStringExtra("registration_id");
					onRegistered(gcmID);
				}
				else if (action.Equals("com.google.android.c2dm.intent.RECEIVE"))
				{
					onMessage(intent.GetStringExtra (MESSAGE_FROM_GCM));
				}
				else if (action.Equals(GoogleCloudMessaging.MessageTypeDeleted))
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

		public abstract void onMessage (string message); 

		public abstract void onError (string error);

		public abstract void onDelete (int deleted);

		public abstract void onRegistered (string gcmID);

		public abstract void onUnregistered (string oldID);

	}
}

