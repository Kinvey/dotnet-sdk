using Kinvey.Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestLocalLibApp.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(IosAppBuilder))]

namespace Kinvey.TestLocalLibApp.iOS
{
    public class IosAppBuilder : IBuilder
    {
        public Client.Builder GetBuilder()
        {
            return new Client.Builder(Kinvey.TestApp.Shared.Constants.Settings.AppKey, Kinvey.TestApp.Shared.Constants.Settings.AppSecret);
        }
    }
}
