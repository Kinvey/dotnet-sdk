using System;
using System.Collections.Generic;
using Kinvey.TestLocalLibApp.Models;

namespace Kinvey.TestLocalLibApp.Pages
{
    public partial class ContractsPage : BasePage
    {
        public ContractsPage()
        {
            InitializeComponent();         
        }

        private async void ContractsPage_OnAppearing(object sender, EventArgs e)
        {
            try
            {
                User user;

                //Verify that the current user already login.
                if (!Client.SharedClient.IsUserLoggedIn())
                {
                    //If not. Login. 
                    user = await User.LoginAsync(Constants.Settings.User, Constants.Settings.Password);
                }
                else
                {
                    //Else. Refresh the user to have the latest data.
                    user = await Client.SharedClient.ActiveUser.RefreshAsync();
                }

                //Updating the label.
                UserLabel.Text = $"Hello {user.UserName} !";

                // Getting an instance of  DataStore.
                var dataStore = DataStore<Contract>.Collection(Constants.Settings.ContractsCollection,
                    DataStoreType.CACHE);

                var contracts = new List<Contract>();

                // The KinveyDelegate onSuccess() method will be called with cache results.                
                var cacheDelegate = new KinveyDelegate<List<Contract>>
                {
                    onSuccess = results => contracts.AddRange(results),
                    onError = async ex => await DisplayMessage(Constants.Exceptions.GeneralExceptionTitle, ex.Message)
                };
                // The method will return the network results.
                await dataStore.FindAsync(cacheResults: cacheDelegate);

                //Binding ListView with data.
                ContractsList.ItemsSource = contracts;
            }
            catch (Exception ex)
            {
                //Popup with exception message.
                await DisplayMessage(Constants.Exceptions.GeneralExceptionTitle, ex.Message);
            }
        }

        private void AddContractButton_OnClicked(object sender, EventArgs e)
        {
            //Navigation to the AddContract page
            Navigation.PushModalAsync(new AddContract());
        }
    }
}
