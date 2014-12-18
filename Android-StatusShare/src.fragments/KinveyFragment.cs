
using System;
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

namespace AndroidStatusShare
{
	public abstract class KinveyFragment : Fragment
	{

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetHasOptionsMenu (true);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup vgroup, Bundle bundle){
			View v = inflater.Inflate (getViewId (), vgroup, false);
			bindViews (v);
			return v;
		}

		public override void OnResume(){
			base.OnResume ();
			populateViews ();
		}

		public abstract int getViewId ();

		public abstract void bindViews (View v);

		public abstract void populateViews ();

	}
}

