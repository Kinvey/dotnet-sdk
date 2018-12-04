using Android.App;
using Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestLocalLibApp.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(FCMService))]
namespace Kinvey.TestLocalLibApp.Droid
{
    [Service]
    public class FCMService : KinveyFCMService, IFCMService 
    {
        public override void onDelete(int deleted)
        {
            throw new System.NotImplementedException();
        }

        public override void onError(string error)
        {
            
        }

        public override void onMessage(string message)
        {
            throw new System.NotImplementedException();
        }

        public override void onRegistered(string registrationId)
        {
            
        }

        public override void onUnregistered(string oldID)
        {
            throw new System.NotImplementedException();
        }

        public void Register(Client client)
        {
            client.FcmPush().Initialize(Application.Context);
        }

        public void UnRegister(Client client)
        {
            client.FcmPush().DisablePush(Application.Context);
        }
    }
}