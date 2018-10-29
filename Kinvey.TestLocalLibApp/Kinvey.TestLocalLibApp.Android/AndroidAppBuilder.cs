using Kinvey.Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestLocalLibApp.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidAppBuilder))]

namespace Kinvey.TestLocalLibApp.Droid
{   
    public class AndroidAppBuilder : IBuilder
    {
        public Client.Builder GetBuilder()
        {
            return new Client.Builder(Kinvey.TestApp.Shared.Constants.Settings.AppKey, Kinvey.TestApp.Shared.Constants.Settings.AppSecret);
        }
    }
}
