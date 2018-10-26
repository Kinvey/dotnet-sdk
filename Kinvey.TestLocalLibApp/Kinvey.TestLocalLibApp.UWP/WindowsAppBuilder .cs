using Kinvey.TestLocalLibApp.Interfaces;
using Kinvey.TestLocalLibApp.UWP;
using Xamarin.Forms;


[assembly: Dependency(typeof(WindowsAppBuilder))]

namespace Kinvey.TestLocalLibApp.UWP
{
    public class WindowsAppBuilder : IBuilder
    {
        public Client.Builder GetBuilder()
        {
            return new Client.Builder(Constants.Settings.AppKey, Constants.Settings.AppSecret).SetFilePath(Windows.Storage.ApplicationData.Current.LocalFolder.Path);
        }
    }
}
