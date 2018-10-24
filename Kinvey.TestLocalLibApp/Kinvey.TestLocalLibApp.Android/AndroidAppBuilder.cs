using Kinvey.TestLocalLibApp.Droid;
using Kinvey.TestLocalLibApp.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidAppBuilder))]

namespace Kinvey.TestLocalLibApp.Droid
{   
    public class AndroidAppBuilder : IBuilder
    {
        public Client.Builder GetBuilder()
        {
            return new Client.Builder(Constants.Settings.AppKey, Constants.Settings.AppSecret);
        }
    }
}
