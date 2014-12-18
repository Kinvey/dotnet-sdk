
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

		private Uri mImageCaptureUri;

		public int width { get; set;}
		public Java.IO.File file { get; set;}


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

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			// make it available in the gallery
			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Android.Net.Uri contentUri = Android.Net.Uri.FromFile(file);
			mediaScanIntent.SetData(contentUri);
			this.SendBroadcast(mediaScanIntent);

			// display in ImageView. We will resize the bitmap to fit the display
			// Loading the full sized image will consume to much memory 
			// and cause the application to crash.
			int height = Resources.DisplayMetrics.HeightPixels;
			bitmap = UpdateEntity.LoadAndResizeBitmap (file.Path, width, height);
		}
	
	}
}