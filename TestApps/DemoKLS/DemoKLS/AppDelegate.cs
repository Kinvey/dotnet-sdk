using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using SQLite.Net.Platform.XamarinIOS;
using Kinvey;

namespace DemoKLS
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

		//string appKey = "kid_S1384V55e", appSecret = "b0a209eaabc54539bc96195b40fb4be7"; // [local] DemoKLS
		string appKey = "kid_HJZHrHi5x", appSecret = "cc9fdb8a9ddc428f889bd96ea59ef2fc"; // DemoKLS

		LoginViewController vc;

		Stream<MedicalDeviceCommand> streamCommand;
		Stream<MedicalDeviceStatus> streamStatus;

		User alice;
		User bob;

		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

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
				.setLogger(delegate (string msg)
				{
					Console.WriteLine(msg);
				});

			cb.Build();

			// create a new window instance based on the screen size
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			if (Client.SharedClient.IsUserLoggedIn())
			{
				var alreadyLoggedInController = new PatientViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;
			}
			else
			{
				vc = new LoginViewController();
				var navController = new UINavigationController(vc);
				Window.RootViewController = navController;
			}

			// make the window visible
			Window.MakeKeyAndVisible();
		}

		public async Task<User> LoginAlice()
		{
			try
			{
				await User.LoginAsync("Alice", "alice");

				var alreadyLoggedInController = new DoctorViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;

				// REALTIME REGISTRATION

				// Register for realtime
				await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

				// Create stream object corresponding to "meddevcmds" stream created on the backend
				streamCommand = new Stream<MedicalDeviceCommand>("device_command");
				streamStatus = new Stream<MedicalDeviceStatus>("device_status");

				// Set up status subscribe delegate
				var streamDelegate = new KinveyStreamDelegate<MedicalDeviceStatus>
				{
					OnError = (err) => Console.WriteLine("STREAM Error: " + err.Message),
					OnNext = (senderID, message) => {
						//Console.WriteLine("STREAM SenderID: " + senderID + " -- Command: " + message.Setting);
						stopwatch.Stop();
						TimeSpan timeForRoundtrip = stopwatch.Elapsed;
						stopwatch.Reset();
						string time = timeForRoundtrip.TotalMilliseconds + " ms";
						InvokeOnMainThread(() => alreadyLoggedInController.ChangeText(message.Setting, time));
					},
					OnStatus = (status) => {
						Console.WriteLine("Status: " + status.Status);
						Console.WriteLine("Status Message: " + status.Message);
						Console.WriteLine("Status Channel: " + status.Channel);
						Console.WriteLine("Status Channel Group: " + status.ChannelGroup);
					}
				};

				// Subscribe to status stream for Bob
				var criteria = new UserDiscovery();
				criteria.FirstName = "Bob";
				var lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				bob = lookup[0];

				await streamStatus.Subscribe(bob.Id, streamDelegate);

				criteria.FirstName = "Alice";
				lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				alice = lookup[0];

				//// Subscribe to status stream for Charlie
				//criteria.FirstName = "Charlie";
				//lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				//User charlie = lookup[0];

				//await streamStatus.Subscribe(charlie.Id, streamDelegate);
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

		public async Task<User> LoginBob()
		{
			try
			{
				await User.LoginAsync("Bob", "bob");

				var alreadyLoggedInController = new PatientViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;

				// REALTIME REGISTRATION
				int settingValue = 70;

				// Register for realtime
				await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

				// Create stream object corresponding to "meddevcmds" stream created on the backend
				streamCommand = new Stream<MedicalDeviceCommand>("device_command");
				streamStatus = new Stream<MedicalDeviceStatus>("device_status");

				// Set up command subscribe delegate
				var streamDelegate = new KinveyStreamDelegate<MedicalDeviceCommand>
				{
					OnError = (err) => Console.WriteLine("STREAM Error: " + err.Message),
					OnNext = async (senderID, message) => {
						//Console.WriteLine("STREAM SenderID: " + senderID + " -- Command: " + message.Command);
						if (message.Command == MedicalDeviceCommand.EnumCommand.INCREMENT)
						{
							settingValue++;
						}
						else
						{
							settingValue--;
						}
						InvokeOnMainThread(() => alreadyLoggedInController.ChangeText(senderID, settingValue.ToString()));
						await this.PublishStatus(settingValue.ToString());
					},
					OnStatus = (status) => {
						Console.WriteLine("Status: " + status.Status);
						Console.WriteLine("Status Message: " + status.Message);
						Console.WriteLine("Status Channel: " + status.Channel);
						Console.WriteLine("Status Channel Group: " + status.ChannelGroup);
					}
				};

				// Subscribe to command stream for Alice
				var criteria = new UserDiscovery();
				criteria.FirstName = "Alice";
				var lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				alice = lookup[0];

				await streamCommand.Subscribe(alice.Id, streamDelegate);

				// Alice
				criteria.FirstName = "Bob";
				lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				bob = lookup[0];

				// Set and send initial setting
				InvokeOnMainThread(() => alreadyLoggedInController.ChangeText(string.Empty, settingValue.ToString()));
				await this.PublishStatus(settingValue.ToString());
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

		//public async Task<User> LoginCharlie()
		//{
		//	try
		//	{
		//		await User.LoginAsync("Charlie", "charlie");

		//		var alreadyLoggedInController = new PatientViewController();
		//		var navController = new UINavigationController(alreadyLoggedInController);
		//		Window.RootViewController = navController;

		//		// REALTIME REGISTRATION

		//		// Register for realtime
		//		await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

		//		// Create stream object corresponding to "meddevcmds" stream created on the backend
		//		streamCommand = new Stream<MedicalDeviceCommand>("device_command");
		//		streamStatus = new Stream<MedicalDeviceStatus>("device_status");
		//	}
		//	catch (KinveyException e)
		//	{
		//		if (e.ErrorCategory == EnumErrorCategory.ERROR_REALTIME)
		//		{
		//			Console.WriteLine("VRG (exception caught) Exception from Realtime operation");
		//		}
		//		Console.WriteLine("VRG (exception caught) Exception Error -> " + e.Error);
		//		Console.WriteLine("VRG (exception caught) Exception Description -> " + e.Description);
		//		Console.WriteLine("VRG (exception caught) Exception Debug -> " + e.Debug);
		//		Console.WriteLine("VRG (exception caught) Exception Request ID -> " + e.RequestID);
		//	}

		//	return Client.SharedClient.ActiveUser;
		//}

		public async Task<User> LoginDan()
		{
			try
			{
				await User.LoginAsync("Dan", "dan");

				var alreadyLoggedInController = new PatientViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;

				// REALTIME REGISTRATION

				// Register for realtime
				await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();

				// Create stream object corresponding to "meddevcmds" stream created on the backend
				streamCommand = new Stream<MedicalDeviceCommand>("device_command");
				streamStatus = new Stream<MedicalDeviceStatus>("device_status");

				// Get user IDs for granting stream access
				var criteria = new UserDiscovery();

				// Alice
				criteria.FirstName = "Alice";
				var lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				User alice = lookup[0];

				// Bob
				criteria.FirstName = "Bob";
				lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				User bob = lookup[0];

				//// Charlie
				//criteria.FirstName = "Charlie";
				//lookup = await Client.SharedClient.ActiveUser.LookupAsync(criteria);
				//User charlie = lookup[0];

				// Grant stream access for the device stream
				var streamACLDeviceAlice = new StreamAccessControlList();
				streamACLDeviceAlice.Publishers.Add(alice.Id);
				streamACLDeviceAlice.Subscribers.Add(bob.Id);
				bool resultGrantDevice = await streamCommand.GrantStreamAccess(alice.Id, streamACLDeviceAlice);

				//var streamACLDeviceBob = new StreamAccessControlList();
				//streamACLDeviceBob.Subscribers.Add(bob.Id);
				//bool resultGrantDeviceBob = await streamCommand.GrantStreamAccess(alice.Id, streamACLDeviceBob);

				// Grant stream access for the status stream
				var streamACLStatusBob = new StreamAccessControlList();
				streamACLStatusBob.Publishers.Add(bob.Id);
				streamACLStatusBob.Publishers.Add(alice.Id);
				streamACLStatusBob.Subscribers.Add(alice.Id);
				streamACLStatusBob.Subscribers.Add(bob.Id);
				bool resultGrantStatusBob = await streamStatus.GrantStreamAccess(bob.Id, streamACLStatusBob);

				var streamACLStatusAlice = new StreamAccessControlList();
				streamACLStatusAlice.Subscribers.Add(alice.Id);
				streamACLStatusAlice.Subscribers.Add(bob.Id);
				streamACLStatusAlice.Publishers.Add(bob.Id);
				streamACLStatusAlice.Publishers.Add(alice.Id);
				bool resultGrantStatusAlice = await streamStatus.GrantStreamAccess(alice.Id, streamACLStatusAlice);
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

		public async Task<User> Login(string user, string pass)
		{
			try
			{
				await User.LoginAsync(user, pass);

				var alreadyLoggedInController = new PatientViewController();
				var navController = new UINavigationController(alreadyLoggedInController);
				Window.RootViewController = navController;

				// REALTIME REGISTRATION

				// Register for realtime
				await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();



				// REALTIME USER-TO-USER COMMUNICATION

				//// Create stream object corresponding to "meddevcmds" stream created on the backend
				//stream = new Stream<MedicalDeviceCommand>("device_command");
				//var streamStatus = new Stream<MedicalDeviceStatus>("device_status");

				//// Grant stream access to active user for both publish and subscribe actions
				//var streamACL = new StreamAccessControlList();
				//streamACL.Publishers.Add(Client.SharedClient.ActiveUser.Id);
				//streamACL.Subscribers.Add(Client.SharedClient.ActiveUser.Id);
				//bool resultGrant = await stream.GrantStreamAccess(Client.SharedClient.ActiveUser.Id, streamACL);

				//// Subscribe to user-to-user stream
				//await stream.Subscribe(Client.SharedClient.ActiveUser.Id, new KinveyRealtimeDelegate<MedicalDeviceCommand>
				//{
				//	OnError = (err) => Console.WriteLine("STREAM Error: " + err.Message),
				//	OnNext = (message) => {
				//		Console.WriteLine("STREAM SenderID: " + message.SenderID + " -- Command: " + message.Command);
				//		InvokeOnMainThread(() => alreadyLoggedInController.ChangeText(message.SenderID, message.Command));
				//	},
				//	OnStatus = (status) => {
				//		Console.WriteLine("Status: " + status.Status);
				//		Console.WriteLine("Status Message: " + status.Message);
				//		Console.WriteLine("Status Channel: " + status.Channel);
				//		Console.WriteLine("Status Channel Group: " + status.ChannelGroup);
				//	}
				//});
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
			//await stream.Unsubscribe(Client.SharedClient.ActiveUser.Id);
			Client.SharedClient?.ActiveUser?.UnregisterRealtimeAsync();
			Client.SharedClient?.ActiveUser?.Logout();
			var logInController = new LoginViewController();
			var navController = new UINavigationController(logInController);
			Window.RootViewController = navController;
		}

		public async Task PublishCommand(MedicalDeviceCommand.EnumCommand command)
		{
			var mdc = new MedicalDeviceCommand();
			mdc.Command = command;
			stopwatch.Start();
			bool success = await streamCommand.Publish(bob.Id, mdc);
		}

		public async Task PublishStatus(string setting)
		{
			var mds = new MedicalDeviceStatus();
			mds.Setting = setting;
			bool success = await streamStatus.Publish(alice.Id, mds);
		}
	}
}

