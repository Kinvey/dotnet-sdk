using Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestLocalLibApp.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(IOSPushService))]
namespace Kinvey.TestLocalLibApp.iOS
{
    public class IOSPushService : IIOSPushService
    {
        public void Register()
        {
            Client.SharedClient.Push().RegisterForToken();
        }

        public void UnRegister()
        {
            Client.SharedClient.Push().DisablePush();
        }
    }
}
