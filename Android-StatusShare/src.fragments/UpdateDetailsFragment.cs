
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

namespace AndroidStatusShare
{
	public class UpdateDetailsFragment : KinveyFragment
	{

		private ImageView image;
		private TextView text;
		private TextView author;

		private UpdateEntity entity;

		public override int getViewId ()
		{
			return Resource.Layout.fragment_update_details;
		}

		public override void bindViews (View v)
		{
			image = v.FindViewById<ImageView> (Resource.Id.update_image);
			text = v.FindViewById<TextView> (Resource.Id.update_text);
			author = v.FindViewById<TextView> (Resource.Id.update_author);

		}

		public override void populateViews ()
		{
			image.SetImageBitmap (entity.thumbnail);
			text.Text = entity.text;
			author.Text = entity.authorName;

		}


		public static UpdateDetailsFragment newInstance(StatusShare activity, int position){

			UpdateDetailsFragment ret = new UpdateDetailsFragment ();

			ret.entity = activity.shareList [position];

			return ret;

		}
	}
}

