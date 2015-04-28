using System;
using KinveyXamarin;
using UIKit;
using System.Threading;
using Foundation;

namespace KinveyXamariniOS
{
	public class Push : AbstractPush
	{

		private const string APN_Token = "APNToken"; 
		
		public Push (Client client) : base(client){}

		public void Initialize(string deviceToken){

			NSUserDefaults.StandardUserDefaults.SetString(deviceToken, APN_Token); 
			NSUserDefaults.StandardUserDefaults.Synchronize ();

			ThreadPool.QueueUserWorkItem (o => {
				EnablePushViaRest ("ios", deviceToken).Execute();
			
			});
		}

		public void DisablePush(){
			
			string value = NSUserDefaults.StandardUserDefaults.StringForKey(APN_Token);
			if (value == null || value.Length == 0) {
				//this device has not already registered for push
				return;
			}

			ThreadPool.QueueUserWorkItem (o => {
				DisablePushViaRest("ios", value).Execute();
			});
		}

	}
}

