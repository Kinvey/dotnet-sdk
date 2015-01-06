using System;
using System.Drawing;

using Foundation;
using UIKit;
using KinveyXamarin;

namespace iOSTestDrive
{
	public partial class iOS_TestDriveViewController : UIViewController
	{

		private Client myClient;
		private const string stableId = "my ID is unique!";
		private const string COLLECTION = "TestDrive";

		private InMemoryCache<MyEntity> cache;

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public iOS_TestDriveViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		#region View lifecycle

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			myClient = new Client.Builder ("kid_PeYFqjBcBJ", "3fee066a01784e2ab32a255151ff761b").build ();

			await myClient.User ().LoginAsync ();

			Console.WriteLine ("logged in as: " + myClient.User().UserName);

			cache = new InMemoryCache<MyEntity> ();
			
			saveButton.TouchUpInside += async (object sender, EventArgs e) => {

				AsyncAppData<MyEntity> entityCollection = myClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
				entityCollection.setCache(cache, CachePolicy.CACHE_FIRST);

				MyEntity ent = new MyEntity();
				ent.Email = "test@tester.com";
				ent.Name = "James Dean";
				ent.ID = stableId;

				MyEntity entity = await entityCollection.SaveAsync(ent);

				Console.WriteLine("saved: " + entity);
			};

			loadButton.TouchUpInside += async (object sender, EventArgs e) => {

				AsyncAppData<MyEntity> entityCollection = myClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));

				MyEntity entity = await entityCollection.GetEntityAsync(stableId);

				Console.WriteLine("loaded: " + entity);
			};

			queryButton.TouchUpInside += async (object sender, EventArgs e) => {

				AsyncAppData<MyEntity> entityCollection = myClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));

				MyEntity[] entities = await entityCollection.GetAsync();

				Console.WriteLine("god: " + entities.Length);
			};

			loadCacheButton.TouchUpInside += async (object sender, EventArgs e) => {

				AsyncAppData<MyEntity> entityCollection = myClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));

				entityCollection.setCache(cache, CachePolicy.CACHE_FIRST);

				MyEntity entity = await entityCollection.GetEntityAsync(stableId);

				Console.WriteLine("loaded: " + entity);
			};
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		#endregion
	}
}

