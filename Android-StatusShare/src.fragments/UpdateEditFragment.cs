
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
using KinveyXamarin;
using System.IO;

namespace AndroidStatusShare
{
	public class UpdateEditFragment : KinveyFragment
	{

		private ImageView attachmentImage;

		private EditText updateText;
		private Bitmap image;

		public static Java.IO.File dir;     

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

			CreateDirectoryForPictures();

			attachmentImage.Click += ((object sender, System.EventArgs e) => {
				Intent intent = new Intent (MediaStore.ActionImageCapture);

				((StatusShare)Activity).file = new Java.IO.File (dir, String.Format ("update_{0}.jpg", Guid.NewGuid ()));
				((StatusShare)Activity).width = attachmentImage.Width;

				intent.PutExtra (MediaStore.ExtraOutput, Android.Net.Uri.FromFile (((StatusShare)Activity).file));

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
			dir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "Status-Share");
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
			Android.Net.Uri contentUri = Android.Net.Uri.FromFile(((StatusShare)Activity).file);
			mediaScanIntent.SetData(contentUri);
			Activity.SendBroadcast(mediaScanIntent);

			// display in ImageView. We will resize the bitmap to fit the display
			// Loading the full sized image will consume to much memory 
			// and cause the application to crash.
			int height = Resources.DisplayMetrics.HeightPixels;
			((StatusShare)Activity).bitmap = UpdateEntity.LoadAndResizeBitmap (((StatusShare)Activity).file.Path, ((StatusShare)Activity).width, height);
			attachmentImage.SetImageResource (Android.Resource.Color.Transparent);
			attachmentImage.SetImageBitmap (((StatusShare)Activity).bitmap);

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
			entity.text = updateText.Text;
			entity.author = new KinveyReference<KinveyXamarin.User> ("user", KinveyService.getCurrentUserId());

			byte[] bytes;
			using (var stream = new MemoryStream())
			{
				((StatusShare)Activity).bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
				bytes = stream.ToArray();
			}
				

			KinveyService.saveUpdate (entity, bytes, new KinveyXamarin.KinveyDelegate<UpdateEntity>(){

				onSuccess =  (update) => { 
					Activity.RunOnUiThread (() => {
						Toast.MakeText(this.Activity, "uplaoded: " + update.ID, ToastLength.Short).Show();
						((StatusShare)this.Activity).ReplaceFragment (new ShareListFragment (), false);
					});
				},
				onError = (error) => {
					Activity.RunOnUiThread (() => {
						Toast.MakeText(this.Activity, "something went wrong: " + error.Message, ToastLength.Short).Show();
					});
				}


			});
		}

	}
}

