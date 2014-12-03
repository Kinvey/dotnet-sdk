using System;
using Android.Widget;
using Android.Content;
using System.Collections.Generic;
using Android.Views;

namespace AndroidStatusShare
{
	public class UpdateAdapter : ArrayAdapter<UpdateEntity>
	{

		private LayoutInflater inflater;


		public UpdateAdapter (Context context, UpdateEntity[] objects, LayoutInflater inflater) : base(context, 0, objects)
		{
			this.inflater = inflater;
		}
	

		public override View GetView(int position, View convertView, ViewGroup parent){

			UpdateViewHolder holder = null;

			if (convertView == null) {
				convertView = inflater.Inflate (Resource.Layout.row_update, null);
				holder = new UpdateViewHolder (convertView);
				convertView.SetTag (0, holder);
			}

			holder = (UpdateViewHolder) convertView.GetTag (0);

			UpdateEntity rowData = GetItem (position);

			if (rowData.text != null) {
				holder.getBlurb ().Text = rowData.text;
			}

			if (rowData.author != null) {
				holder.getAuthor ().Text = rowData.authorName;
			}

			if (rowData.getWhen () != null) {
				holder.getWhen ().Text = rowData.getWhen ();
			}

			if (rowData.thumbnail != null) {
				holder.getAttachment ().SetImageBitmap (rowData.thumbnail);
			}

			return convertView;
		}

	}

	class UpdateViewHolder : Java.Lang.Object{

		private ImageView attachment;
		private TextView blurb;
		private TextView author;
		private TextView when;
		private View row;

		public UpdateViewHolder(View row){
			this.row = row;
		}

		public ImageView getAttachment()
		{
			if (attachment == null) {
				attachment = row.FindViewById<ImageView> (Resource.Id.row_update_image);
			}
			return attachment;
		
		}

		public TextView getBlurb()
		{
			if (blurb == null) {
				blurb = row.FindViewById<TextView> (Resource.Id.row_update_text);
			}
			return blurb;

		}

		public TextView getAuthor()
		{
			if (author == null) {
				author = row.FindViewById<TextView> (Resource.Id.row_update_author);
			}
			return author;

		}

		public TextView getWhen() 
		{
			if (when == null) {
				when = row.FindViewById<TextView> (Resource.Id.row_update_time);
			}
			return when;

		}

	}
}

