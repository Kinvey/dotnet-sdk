using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Threading.Tasks;
using SQLite.Net.Platform.XamarinAndroid;
using KinveyXamarin;

namespace testdroidapp
{
	[Activity(Label = "testdroidapp", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		int count = 1;
		Client myClient;

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.myButton);

			button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

			Client.Builder builder = new Client.Builder("kid_b1d6IY_x7l", "079412ee99f4485d85e6e362fb987de8")
								 //			myClient = new Client.Builder ("kid_ZkPDb_34T", "c3752d5079f34353ab89d07229efaf63") // MIC-SAML-TEST
								 .setFilePath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal))
								 .setOfflinePlatform(new SQLitePlatformAndroid())
				.setLogger(delegate (string msg) { Console.WriteLine(msg); });
				myClient = await builder.Build();

			await DoStuff();

		}

		private async Task<User> DoStuff()
		{
			//			Dictionary<string, JToken> attr = new Dictionary<string, JToken> ();
			//			email.Add ("email", "gob@bluth.com");
			//			attr.Add ("my_field", "blah blah");
			//			var newuser = await myClient. ().CreateAsync ("George Michael Bluth", "cousin", attr);
			//			var newuser2 = await myClient.CurrentUser.CreateAsync ("Tobias Funke", "actor");
			//			Dictionary<string, JToken> last_name = new Dictionary<string, JToken>();
			//			last_name.Add("last_name", "Bluth");
			//			await myClient.CurrentUser.CreateAsync ("Lindsay Bluth", "me", last_name);
			//			await myClient.CurrentUser.CreateAsync ("Maeby Bluth", "Surely", last_name);

			User user = myClient.ActiveUser;
			try
			{
				if (!myClient.IsUserLoggedIn())
				{
					user = await User.LoginAsync("test", "test", myClient);

					//					myClient.CurrentUser.LoginWithAuthorizationCodeLoginPage("kinveyAuthDemo://", new KinveyMICDelegate<User>{
					//						onSuccess = (loggedInUser) => { user = loggedInUser; },
					//						onError = (e) => { Console.WriteLine("Error with MIC Login"); },
					//						onReadyToRender = (url) => { UIApplication.SharedApplication.OpenUrl(new NSUrl(url)); }
					//					});
				}


				string str = "Finished Launching.";
				Console.WriteLine("VRG : " + str);
				Console.WriteLine("VRG: Logged in as: " + myClient.ActiveUser.Id);

			}
			catch (Exception e)
			{
				string msg = e.Message;
			}

			return user;
		}
	}
}


