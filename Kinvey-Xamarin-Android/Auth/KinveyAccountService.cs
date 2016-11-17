using System;
using System.Collections.Generic;
using Android.Accounts;
using Android.App;
using Android.Content;
using Newtonsoft.Json;
using KinveyXamarin;

namespace Kinvey
{
	/// <summary>
	/// Kinvey account service.
	/// </summary>
	[Service]
	public class KinveyAccountService : Service
	{
		private KinveyAccountAuthenticator kinveyAccountAuthenticator;

		/// <summary>
		/// Ons the create.
		/// </summary>
		public override void OnCreate()
		{
			base.OnCreate();
			kinveyAccountAuthenticator = new KinveyAccountAuthenticator(this);
		}

		/// <summary>
		/// Ons the bind.
		/// </summary>
		/// <returns>The bind.</returns>
		/// <param name="intent">Intent.</param>
		public override Android.OS.IBinder OnBind(Intent intent)
		{
			return kinveyAccountAuthenticator.IBinder;
		}

		/// <summary>
		/// Ons the start command.
		/// </summary>
		/// <returns>The start command.</returns>
		/// <param name="intent">Intent.</param>
		/// <param name="flags">Flags.</param>
		/// <param name="startId">Start identifier.</param>
		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			return base.OnStartCommand(intent, flags, startId);
		}
	}
}
