using System;
using Kinvey;
using UIKit;
using System.Threading;
using Foundation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kinvey
{
	public class Push : AbstractPush
	{

		private const string APN_Token = "APNToken"; 
		
		public Push (Client client) : base(client){}

		public void RegisterForToken(){
			if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
				var pushSettings = UIUserNotificationSettings.GetSettingsForTypes (
					UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
					new NSSet ());

				UIApplication.SharedApplication.RegisterUserNotificationSettings (pushSettings);
				UIApplication.SharedApplication.RegisterForRemoteNotifications ();
			} else {
				UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
				UIApplication.SharedApplication.RegisterForRemoteNotificationTypes (notificationTypes);
			}
		
		}

        [Obsolete("This method has been deprecated. Please use InitializeAsync(deviceToken) instead.")]
        public async void Initialize(string deviceToken){
			if (deviceToken == null) {
				Logger.Log ("Cannot Initialize for push, device Token cannot be null!");
				return;
			}

			if (deviceToken.StartsWith ("<")) {
				deviceToken = deviceToken.Substring (1, deviceToken.Length - 1);
			}

			if (deviceToken.EndsWith (">")) {
				deviceToken = deviceToken.Substring (0, deviceToken.Length - 1);
			}

			deviceToken = Regex.Replace (deviceToken, @"\s+", "");
			deviceToken = deviceToken.ToUpper ();

			NSUserDefaults.StandardUserDefaults.SetString(deviceToken, APN_Token); 
			NSUserDefaults.StandardUserDefaults.Synchronize ();
			try{
			  await EnablePushAsync("ios", deviceToken);
			}catch(Exception e){
//				Console.WriteLine ("wtf");
				//throw e;
			}

//			ThreadPool.QueueUserWorkItem (o => {
//				try{
//					EnablePushViaRest ("ios", deviceToken).Execute();
//				} catch (Exception e){
//					delegates.onError(e);
//					throw e;
//				}
//			
//			});
		}

        public async Task InitializeAsync(string deviceToken)
        {
            if (deviceToken == null)
            {
                Logger.Log("Cannot Initialize for push, device Token cannot be null!");
                return;
            }

            if (deviceToken.StartsWith("<"))
            {
                deviceToken = deviceToken.Substring(1, deviceToken.Length - 1);
            }

            if (deviceToken.EndsWith(">"))
            {
                deviceToken = deviceToken.Substring(0, deviceToken.Length - 1);
            }

            deviceToken = Regex.Replace(deviceToken, @"\s+", "");
            deviceToken = deviceToken.ToUpper();

            NSUserDefaults.StandardUserDefaults.SetString(deviceToken, APN_Token);
            NSUserDefaults.StandardUserDefaults.Synchronize();
            try
            {
                await EnablePushAsync("ios", deviceToken);
            }
            catch (Exception e)
            {

            }
        }

        [Obsolete("This method has been deprecated. Please use DisablePushAsync() instead.")]
        public void DisablePush(){
			
			string value = NSUserDefaults.StandardUserDefaults.StringForKey(APN_Token);
			if (value == null || value.Length == 0) {
				Logger.Log ("Cannot Disable Push, this device has not already registered");
				return;
			}



			ThreadPool.QueueUserWorkItem (o => {
//				try{
					DisablePushViaRest("ios", value).Execute();
//				} catch (Exception e){
//					throw e;
//				}
			});
		}

        public async Task DisablePushAsync()
        {
            string value = NSUserDefaults.StandardUserDefaults.StringForKey(APN_Token);
            if (value == null || value.Length == 0)
            {
                Logger.Log("Cannot Disable Push, this device has not already registered");
                return;
            }

            await DisablePushAsync("ios", value);
        }

    }
}

