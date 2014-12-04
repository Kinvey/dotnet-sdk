
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
using KinveyXamarin;

namespace AndroidStatusShare
{
	public class ShareListFragment : KinveyFragment
	{

		private ListView list;
		private ProgressBar loading;
		private TextView empty;
		private UpdateAdapter adapter;


		public override int getViewId ()
		{
			return Resource.Layout.fragment_updates_list;
		}

		public override void bindViews (View v)
		{

			list = v.FindViewById<ListView> (Resource.Id.updateList);
			loading = v.FindViewById<ProgressBar> (Resource.Id.updateProgress);
			empty = v.FindViewById<TextView> (Resource.Id.empty_list);

			empty.Visibility = ViewStates.Gone;

			//loadUpdates ();

		}

		public override void populateViews ()
		{
			loadUpdates ();
		}

		private void loadUpdates(){
			loading.Visibility = ViewStates.Visible;
			empty.Visibility = ViewStates.Gone;

			KinveyService.getUpdates (new KinveyDelegate<UpdateEntity[]>{ 
				onSuccess =  (updates) => { 

					((StatusShare)this.Activity).shareList = updates;
					setAdapter();


				},
				onError = (error) => {
				
				
				}
			});


		}

		private void setAdapter(){
			loading.Visibility = ViewStates.Gone;

			if (((StatusShare)this.Activity).shareList == null || ((StatusShare)this.Activity).shareList.Length == 0) {
				empty.Visibility = ViewStates.Visible;

			} else {
				this.adapter = new UpdateAdapter (this.Activity, ((StatusShare)this.Activity).shareList, this.Activity.LayoutInflater);
				this.list.Adapter = this.adapter;
				this.list.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {

					((StatusShare)this.Activity).ReplaceFragment( UpdateDetailsFragment.newInstance((StatusShare)this.Activity, e.Position), true);

				};
			}
		}
			
		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater) {
			menu.Clear();
			inflater.Inflate(Resource.Menu.menu_sharelist, menu);
		}
			
		public override bool OnOptionsItemSelected(IMenuItem item) {
			switch (item.ItemId){

			case Resource.Id.menu_status_post:
				((StatusShare) Activity).ReplaceFragment(new UpdateEditFragment(), true);
				return (true);

			case Resource.Id.menu_refresh:
				adapter = null;
				loadUpdates();
				return true;

			case Resource.Id.menu_sign_out:
				KinveyService.logout ();
				Activity.Finish ();

				return true;
			}

			return base.OnOptionsItemSelected(item);
		}

	}
}

