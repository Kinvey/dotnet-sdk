
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Provider;
using Android.Content.PM;
using Java.IO;
using Android.Net;
using System;

namespace AndroidStatusShare
{
	public class UpdateEditFragment : KinveyFragment
	{

		private ImageView attachmentImage;

		private EditText updateText;
		private Bitmap image;

		public static File file;
		public static File dir;     
		public static Bitmap bitmap;

		public UpdateEntity entity;

		public override void OnCreate(Bundle saved){
			base.OnCreate (saved);
			entity = new UpdateEntity ();
		}

		public override int getViewId ()
		{
			return Resource.Layout.fragment_write_update;
		}

		public override void bindViews (View v)
		{
			attachmentImage = v.FindViewById<ImageView> (Resource.Id.preview);
			updateText = v.FindViewById<EditText> (Resource.Id.update);

			attachmentImage.Click += ((object sender, System.EventArgs e) => {
				Intent intent = new Intent (MediaStore.ActionImageCapture);

				file = new File (dir, String.Format ("update_{0}.jpg", Guid.NewGuid ()));

				intent.PutExtra (MediaStore.ExtraOutput, Android.Net.Uri.FromFile (file));

				StartActivityForResult (intent, 0);

			});

		}

		public override void populateViews (){}

		private bool IsThereAnAppToTakePictures()
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			PackageManager pm = Activity.PackageManager;
			IList<ResolveInfo> availableActivities = pm.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		private void CreateDirectoryForPictures()
		{
			dir = new File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "Status-Share");
			if (!dir.Exists())
			{
				dir.Mkdirs();
			}
		}

		public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			// make it available in the gallery
			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Android.Net.Uri contentUri = Android.Net.Uri.FromFile(file);
			mediaScanIntent.SetData(contentUri);
			Activity.SendBroadcast(mediaScanIntent);

			// display in ImageView. We will resize the bitmap to fit the display
			// Loading the full sized image will consume to much memory 
			// and cause the application to crash.
			int height = Resources.DisplayMetrics.HeightPixels;
			int width = attachmentImage.Width ;
			bitmap = UpdateEntity.LoadAndResizeBitmap (file.Path, width, height);
		}


		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater) {
			menu.Clear();
			inflater.Inflate(Resource.Menu.menu_editshare, menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			switch (item.ItemId) {
			case Resource.Id.menu_send_post:
				doUpdate();
				break;
			}

			return base.OnOptionsItemSelected(item);
		}

		public void doUpdate(){
			KinveyService.saveUpdate (entity, new byte[0], new KinveyXamarin.KinveyDelegate<UpdateEntity>(){




			}, new KinveyXamarin.KinveyDelegate<KinveyXamarin.FileMetaData>(){




			});
		}

	}
}

