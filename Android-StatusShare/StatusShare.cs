
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

namespace AndroidStatusShare
{

	public static final String TAG = "Kinvey - StatusShare";

	public static final String COL_UPDATES = "Updates";
	public static final String COL_COMMENTS = "Comments";

	private static final int PICK_FROM_CAMERA = 1;
	private static final int PICK_FROM_FILE = 2;

	private Uri mImageCaptureUri;

	public Bitmap bitmap = null;
	public String path = null;

	private List<UpdateEntity> shareList;


	[Activity (Label = "StatusShare", MainLauncher = true, Icon = "@drawable/icon")]		
	public class StatusShare : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
		}

		public void replaceFragment(SherlockFragment frag, boolean addToBackStack) {
			FragmentTransaction ft = getSupportFragmentManager().beginTransaction();
			ft.replace(R.id.fragmentBox, frag);
			if (addToBackStack) {
				ft.addToBackStack(frag.toString());
			}
			ft.commit();
		}


	
	}
}