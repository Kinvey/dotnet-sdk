using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using SQLite.Net.Platform.XamarinIOS;
using Kinvey;

namespace Realtime
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window
		{
			get;
			set;
		}

		string appKey = "kid_BJYSU7Yug", appSecret = "9dc0806a28df425999f73767554d068d"; // [local] RealtimeTestApp

		LoginViewController vc;

		Stream<MedicalDeviceCommand> stream;

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method

			BuildClient();

			return true;
		}

		public override void OnResignActivation(UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground(UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground(UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated(UIApplication application)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		}

		public override void WillTerminate(UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}

		public void BuildClient()
		{
			Client.Builder cb = new Client.Builder(appKey, appSecret)
				.setFilePath(NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].ToString())
				.setOfflinePlatform(new SQLitePlatformIOS())
				.setBaseURL("http://127.0.0.1:7007/")
				.setLogger(delegate(string msg) {
					Console.WriteLine(msg);
				});

			cb.Build();

			// create a new window instance based on the screen size
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			if (Client.SharedClient.IsUserLoggedIn())
			{
				var alreadyLoggedInController = new Realtime.PubSubViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;
			}
			else
			{
				vc = new Realtime.LoginViewController();
				var navController = new UINavigationController(vc);
				Window.RootViewController = navController;
			}

			// make the window visible
			Window.MakeKeyAndVisible();
		}

		public async Task<User> Login(string user, string pass)
		{
			try
			{
				await User.LoginAsync(user, pass);

				var alreadyLoggedInController = new Realtime.PubSubViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;

				// REALTIME REGISTRATION

				// Register for realtime
				await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();


				// REALTIME COLLECTION SUBSCRIPTION

				// Subscribe to collection for realtime updates
				DataStore<ToDo> store = DataStore<ToDo>.Collection("ToDo", DataStoreType.NETWORK, Client.SharedClient);
				await store.Subscribe(new KinveyDataStoreDelegate<ToDo>
				{
					OnError = (err) => Console.WriteLine("Error: " + err.Message),
					OnNext = (result) => {
						Console.WriteLine("ToDo: Name: " + result.Name + " -- Details: " + result.Details);
						InvokeOnMainThread(() => alreadyLoggedInController.ChangeText(result.Name, result.Details));
					},
					OnStatus = (status) => {
						Console.WriteLine("Status: " + status.Status);
						Console.WriteLine("Status Message: " + status.Message);
						Console.WriteLine("Status Channel: " + status.Channel);
						Console.WriteLine("Status Channel Group: " + status.ChannelGroup);
					}
				});

				// save to collection to trigger realtime update
				var todo = new ToDo();
				todo.Name = "Test Todo";
				todo.Details = "Test Todo Details";
				todo = await store.SaveAsync(todo);


				// REALTIME USER-TO-USER COMMUNICATION

				// Create stream object corresponding to "meddevcmds" stream created on the backend
				stream = new Stream<MedicalDeviceCommand>("meddevcmds");

				// Grant stream access to active user for both publish and subscribe actions
				var streamACL = new StreamAccessControlList();
				streamACL.Publishers.Add(Client.SharedClient.ActiveUser.Id);
				streamACL.Subscribers.Add(Client.SharedClient.ActiveUser.Id);
				bool resultGrant = await stream.GrantStreamAccess(Client.SharedClient.ActiveUser.Id, streamACL);

				// Subscribe to user-to-user stream
				await stream.Subscribe(Client.SharedClient.ActiveUser.Id, new KinveyStreamDelegate<MedicalDeviceCommand>
				{
					OnError = (err) => Console.WriteLine("STREAM Error: " + err.Message),
					OnNext = (senderID, message) => {
						Console.WriteLine("STREAM SenderID: " + senderID + " -- Command: " + message.Command);
						InvokeOnMainThread(() => alreadyLoggedInController.ChangeText(senderID, message.Command));
					},
					OnStatus = (status) => {
						Console.WriteLine("Status: " + status.Status);
						Console.WriteLine("Status Message: " + status.Message);
						Console.WriteLine("Status Channel: " + status.Channel);
						Console.WriteLine("Status Channel Group: " + status.ChannelGroup);
					}
				});
			}
			catch (KinveyException e)
			{
				if (e.ErrorCategory == EnumErrorCategory.ERROR_REALTIME)
				{
					Console.WriteLine("VRG (exception caught) Exception from Realtime operation");
				}
				Console.WriteLine("VRG (exception caught) Exception Error -> " + e.Error);
				Console.WriteLine("VRG (exception caught) Exception Description -> " + e.Description);
				Console.WriteLine("VRG (exception caught) Exception Debug -> " + e.Debug);
				Console.WriteLine("VRG (exception caught) Exception Request ID -> " + e.RequestID);
			}

			return Client.SharedClient.ActiveUser;
		}

		public async Task Logout()
		{
			await stream.Unsubscribe(Client.SharedClient.ActiveUser.Id);
			Client.SharedClient?.ActiveUser?.UnregisterRealtimeAsync();
			Client.SharedClient?.ActiveUser?.Logout();
			var logInController = new Realtime.LoginViewController();
			var navController = new UINavigationController(logInController);
			Window.RootViewController = navController;
		}

		public async Task Publish(string command)
		{
			var mdc = new MedicalDeviceCommand();
			mdc.Command = command;
			bool success = await stream.Publish(Client.SharedClient.ActiveUser.Id, mdc);
		}
	}
}
