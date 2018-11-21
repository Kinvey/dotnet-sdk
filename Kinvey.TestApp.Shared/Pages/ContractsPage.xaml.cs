using System;
using System.Collections.Generic;
using Kinvey.Kinvey.TestApp.Shared.Interfaces;
using Kinvey.Kinvey.TestApp.Shared.Models;
using Xamarin.Forms;

namespace Kinvey.TestApp.Shared.Pages
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
                    user = await User.LoginAsync(Kinvey.TestApp.Shared.Constants.Settings.User, Kinvey.TestApp.Shared.Constants.Settings.Password);
                }
                else
                {
                    //Else. Refresh the user to have the latest data.
                    user = await Client.SharedClient.ActiveUser.RefreshAsync();
                }

                //Updating the label.
                UserLabel.Text = $"Hello {user.UserName} !";

                // Getting an instance of  DataStore.
                var dataStore = DataStore<Contract>.Collection(Kinvey.TestApp.Shared.Constants.Settings.ContractsCollection,
                    DataStoreType.CACHE);

                var contracts = new List<Contract>();

                // The KinveyDelegate onSuccess() method will be called with cache results.                
                var cacheDelegate = new KinveyDelegate<List<Contract>>
                {
                    onSuccess = results => contracts.AddRange(results),
                    onError = async ex => await DisplayMessage(Kinvey.TestApp.Shared.Constants.Exceptions.GeneralExceptionTitle, ex.Message)
                };
                // The method will return the network results.
                await dataStore.FindAsync(cacheResults: cacheDelegate);

                //Binding ListView with data.
                ContractsList.ItemsSource = contracts;
            }
            catch (Exception ex)
            {
                //Popup with exception message.
                await DisplayMessage(Kinvey.TestApp.Shared.Constants.Exceptions.GeneralExceptionTitle, ex.Message);
            }
        }

        private void AddContractButton_OnClicked(object sender, EventArgs e)
        {
            //Navigation to the AddContract page
            Navigation.PushModalAsync(new AddContract());
        }

        private void RegisterGCM_OnClicked(object sender, EventArgs e)
        {
            var gsmService = DependencyService.Get<IGCMService>();
            gsmService.changed += GsmService_changed;
            gsmService.Register(Client.SharedClient);

            

        }

        private void GsmService_changed(object sender, EventArgs e)
        {
            
        }

        private void DisableGCM_OnClicked(object sender, EventArgs e)
        {
            DependencyService.Get<IGCMService>().Disable(Client.SharedClient);
        }
    }
}
