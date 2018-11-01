using Kinvey.Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestNugetPackageLibApp.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(IosAppBuilder))]

namespace Kinvey.TestNugetPackageLibApp.iOS
{
    public class IosAppBuilder : IBuilder
    {
        public Client.Builder GetBuilder()
        {
            return new Client.Builder(Kinvey.TestApp.Shared.Constants.Settings.AppKey, Kinvey.TestApp.Shared.Constants.Settings.AppSecret);
        }
    }
}
