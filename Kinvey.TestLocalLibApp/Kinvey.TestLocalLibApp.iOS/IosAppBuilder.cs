using Kinvey.TestLocalLibApp.iOS;
using Kinvey.TestLocalLibApp.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(IosAppBuilder))]

namespace Kinvey.TestLocalLibApp.iOS
{
    public class IosAppBuilder : IBuilder
    {
        public Client.Builder GetBuilder()
        {
            return new Client.Builder(Constants.Settings.AppKey, Constants.Settings.AppSecret);
        }
    }
}
