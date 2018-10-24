using System;
using Kinvey.TestLocalLibApp.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kinvey.TestLocalLibApp.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddContract : BasePage
    {
		public AddContract ()
		{
			InitializeComponent ();
		}

	    private async void SaveButton_OnClicked(object sender, EventArgs e)
	    {
	        try
	        {
                //Showing notification popup if number or title are empty
                if (string.IsNullOrEmpty(NumberEntryCell.Text) || string.IsNullOrEmpty(TitleEntryCell.Text))
	            {
	                await DisplayMessage(Constants.Exceptions.RequiredFieldsTitle, Constants.Exceptions.RequiredFieldsMessage);
	                return;
	            }

	            // Getting an instance of  DataStore.
                var dataStore = DataStore<Contract>.Collection(Constants.Settings.ContractsCollection,
	                DataStoreType.CACHE);

	            var contract = new Contract { Number = NumberEntryCell.Text, Title = TitleEntryCell.Text };
                // Save an entity. The entity will be saved to the device and your backend. 
	            // If you do not have a network connection, the entity will be stored in local storage, 
	            // to get pushed to the backend when network becomes available.
                await dataStore.SaveAsync(contract);

                //Going back to the main page
	            await Navigation.PopModalAsync();
	        }
	        catch (Exception ex)
	        {
	            //Popup with exception message.
                await DisplayMessage(Constants.Exceptions.GeneralExceptionTitle, ex.Message);
	        }
        }

	    private async void CancelButton_OnClicked(object sender, EventArgs e)
	    {
	        //Going back to the main page
            await Navigation.PopModalAsync();
        }
	}
}