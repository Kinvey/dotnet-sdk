using System;
using System.Collections.Generic;
using Kinvey.Kinvey.TestApp.Shared.Models;
using Xamarin.Forms;
using Plugin.Connectivity;
using System.Linq;
using Kinvey.TestApp.Shared.Interfaces;
using System.Threading.Tasks;

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
                    DataStoreType.AUTO);

                // The method will return the network results if internet connection is available.
                var contracts = await dataStore.FindAsync();

                //Binding ListView with data.
                ContractsList.ItemsSource = contracts;
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

            Platforms.SelectedIndex = 0;
        }

        private void AddContractButton_OnClicked(object sender, EventArgs e)
        {
            //Navigation to the AddContract page
            Navigation.PushModalAsync(new AddContract());
        }

        private async void SubscribeLiveService_OnClicked(object sender, EventArgs e)
        {
            // Listen for connectivity changes.
            CrossConnectivity.Current.ConnectivityChanged += (senderOfConnectivityChanged, args) =>
            {
                // Make sure to log messages.
                Console.WriteLine("Connectivity Changed. IsConnected: " + CrossConnectivity.Current.IsConnected);
            };

            // Listen for connectivity type changes.
            CrossConnectivity.Current.ConnectivityTypeChanged += (senderOfConnectivityChanged, args) =>
            {
                // Make sure to log messages.
                Console.WriteLine("Connectivity Type Changed. Types: " + args.ConnectionTypes.FirstOrDefault());
            };

            try
            {
                if (Client.SharedClient.IsUserLoggedIn())
                {
                    // Then register fresh.
                    await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();
                    var contracts = DataStore<Contract>.Collection(Kinvey.TestApp.Shared.Constants.Settings.ContractsCollection,
                    DataStoreType.AUTO);
                    // Subscribe to a collection.
                    await contracts.Subscribe(new KinveyDataStoreDelegate<Contract>
                    {
                        OnNext = (result) =>
                        {
                            // Handle new real-time messages.
                            Console.WriteLine("KLS Contract title: " + result.Title);
                        },
                        OnStatus = (status) =>
                        {
                            // Handle subscription status changes.
                            Console.WriteLine("KLS Subscription Status Change: " + status.Message);
                        },
                        OnError = (error) =>
                        {
                            // Handle errors.
                            Console.WriteLine("KLS Error: " + error.Message);
                        }
                    });
                }
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

        private async void UnsubscribeLiveService_OnClicked(object sender, EventArgs e)
        {
            if (Client.SharedClient.IsUserLoggedIn())
            {
                try
                {
                    await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();
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
        }

        private async void RegisterPush_OnClickedAsync(object sender, EventArgs e)
        {
            switch (Platforms.SelectedIndex)
            {
                //Android
                case 0:
                    var fcmService = DependencyService.Get<IFCMService>();
                    await fcmService.RegisterAsync(Client.SharedClient);
                    break;
                //IOS
                case 1:
                    var iosPushService = DependencyService.Get<IIOSPushService>();
                    iosPushService.Register();
                    break;
                default:
                    throw new Exception("Wrong index.");
            }
        }

        private async void UnregisterPush_OnClickedAsync(object sender, EventArgs e)
        {
            switch (Platforms.SelectedIndex)
            {
                //Android
                case 0:
                    var fcmService = DependencyService.Get<IFCMService>();
                    await fcmService.UnRegisterAsync(Client.SharedClient);
                    break;
                //IOS
                case 1:
                    var iosPushService = DependencyService.Get<IIOSPushService>();
                    iosPushService.UnRegister();
                    break;
                default:
                    throw new Exception("Wrong index.");
            }
        }
    }
}
