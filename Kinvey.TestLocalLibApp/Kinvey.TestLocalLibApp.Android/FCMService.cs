using Android.App;
using Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestLocalLibApp.Droid;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(FCMService))]
namespace Kinvey.TestLocalLibApp.Droid
{
    [Service]
    public class FCMService : KinveyFCMService, IFCMService 
    {   
        public override void onError(string error)
        {
            
        }

        public override void onMessage(string message)
        {

        }

        public override void onRegistered(string registrationId)
        {
            
        }

        public override void onUnregistered(string oldID)
        {

        }

        public async Task RegisterAsync(Client client)
        {
            await client.FcmPush().InitializeAsync(Application.Context);
        }

        public async Task UnRegisterAsync(Client client)
        {
            await client.FcmPush().DisablePushAsync(Application.Context);
        }
    }
}