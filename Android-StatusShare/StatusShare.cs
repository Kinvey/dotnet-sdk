
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
using Android.Graphics;
using AndroidStatusShare;

namespace AndroidStatusShare
{
	[Activity (Label = "StatusShare", MainLauncher = true, Icon = "@drawable/icon")]		
	public class StatusShare : Activity
	{


		public static string TAG = "Kinvey - StatusShare";

		public static string COL_UPDATES = "Updates";
		public static string COL_COMMENTS = "Comments";

		private static int PICK_FROM_CAMERA = 1;
		private static int PICK_FROM_FILE = 2;

		private Uri mImageCaptureUri;

		public Bitmap bitmap = null;
		public String path = null;

		public UpdateEntity[] shareList { get; set; }



		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.activity_fragment_holder);
			ReplaceFragment (new LoginFragment (), false);

		}

		public void ReplaceFragment(Fragment frag, bool addToBackStack) {
			FragmentTransaction ft = FragmentManager.BeginTransaction ();
			ft.Replace(Resource.Id.fragmentBox, frag);
			if (addToBackStack) {
				ft.AddToBackStack(frag.ToString());
			}
			ft.Commit();
		}


	
	}
}