using System;
using RestSharp;
using Newtonsoft.Json;
using KinveyXamarin;
using System.Collections.Generic;
using Android.Graphics;


namespace AndroidStatusShare
{
	[JsonObject(MemberSerialization.OptIn)]
	public class UpdateEntity
	{
		public UpdateEntity ()
		{
		}

		[JsonProperty("_id")]
		public string ID {get; set;}

		[JsonProperty]
		public string text{get;set;}

		[JsonProperty("_kmd")]
		public KinveyMetaData kmd;

		[JsonProperty(AccessControlList.JSON_FIELD_NAME)]
		public AccessControlList acl;

		[JsonProperty]
		public KinveyReference<User> author;

		//-----displayed inferred fields
		public string authorName;
		public string authorID;
		public string since;
		public Bitmap thumbnail;


		public String getWhen(){
			return "--";
			//TODO calcalate time since _kmd.ect
		}

		public static Bitmap LoadAndResizeBitmap(string fileName, int width, int height)
		{
			// First we get the the dimensions of the file on disk
			BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
			BitmapFactory.DecodeFile(fileName, options);

			// Next we calculate the ratio that we need to resize the image by
			// in order to fit the requested dimensions.
			int outHeight = options.OutHeight;
			int outWidth = options.OutWidth;
			int inSampleSize = 1;

			if (outHeight > height || outWidth > width)
			{
				inSampleSize = outWidth > outHeight
					? outHeight / height
					: outWidth / width;
			}

			// Now we will load the image and have BitmapFactory resize it for us.
			options.InSampleSize = inSampleSize;
			options.InJustDecodeBounds = false;
			Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

			return resizedBitmap;
		}


	}
}