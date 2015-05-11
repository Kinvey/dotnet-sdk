using System;
using KinveyXamarin;
using UIKit;
using System.Threading;
using Foundation;
using KinveyUtils;

namespace KinveyXamariniOS
{
	public class Push : AbstractPush
	{

		private const string APN_Token = "APNToken"; 
		
		public Push (Client client) : base(client){}

		public void Initialize(string deviceToken){
			if (deviceToken == null) {
				Logger.Log ("Cannot Initialize for push, device Token cannot be null!");
			}

			NSUserDefaults.StandardUserDefaults.SetString(deviceToken, APN_Token); 
			NSUserDefaults.StandardUserDefaults.Synchronize ();

			ThreadPool.QueueUserWorkItem (o => {
				EnablePushViaRest ("ios", deviceToken).Execute();
			
			});
		}

		public void DisablePush(){
			
			string value = NSUserDefaults.StandardUserDefaults.StringForKey(APN_Token);
			if (value == null || value.Length == 0) {
				Logger.Log ("Cannot Disable Push, this device has not already registered");
				return;
			}

			ThreadPool.QueueUserWorkItem (o => {
				DisablePushViaRest("ios", value).Execute();
			});
		}

	}
}

