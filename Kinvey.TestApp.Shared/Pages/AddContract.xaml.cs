using System;
using Kinvey.Kinvey.TestApp.Shared.Models;
using Xamarin.Forms.Xaml;

namespace Kinvey.TestApp.Shared.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddContract : BasePage
    {
		public AddContract()
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
	                await DisplayMessage(Kinvey.TestApp.Shared.Constants.Exceptions.RequiredFieldsTitle, Kinvey.TestApp.Shared.Constants.Exceptions.RequiredFieldsMessage);
	                return;
	            }

	            // Getting an instance of  DataStore.
                var dataStore = DataStore<Contract>.Collection(Kinvey.TestApp.Shared.Constants.Settings.ContractsCollection,
                    DataStoreType.CACHE);

	            var contract = new Contract { Number = NumberEntryCell.Text, Title = TitleEntryCell.Text };
                // Save an entity. The entity will be saved to the device and your backend. 
	            // If you do not have a network connection, the entity will be stored in local storage, 
	            // to get pushed to the backend when network becomes available.
                await dataStore.SaveAsync(contract);

                //Going back to the main page
	            await Navigation.PopModalAsync();
	        }
            catch (KinveyException kinveyException)
            {
                // Handle any Kinvey exception.
                await DisplayMessage(Kinvey.TestApp.Shared.Constants.Exceptions.KinveyExceptionTitle, kinveyException.Message);
            }
            catch (Exception generalException)
            {
                // Handle any General exception.
                await DisplayMessage(Kinvey.TestApp.Shared.Constants.Exceptions.GeneralExceptionTitle, generalException.Message);
            }
        }

	    private async void CancelButton_OnClicked(object sender, EventArgs e)
	    {
	        //Going back to the main page
            await Navigation.PopModalAsync();
        }
	}
}