using Kinvey.Kinvey.TestApp.Shared.Interfaces;
using Kinvey.TestLocalLibApp.UWP;
using Xamarin.Forms;


[assembly: Dependency(typeof(WindowsAppBuilder))]

namespace Kinvey.TestLocalLibApp.UWP
{
    public class WindowsAppBuilder : IBuilder
    {
        public Client.Builder GetBuilder()
        {
            return new Client.Builder(Kinvey.TestApp.Shared.Constants.Settings.AppKey, Kinvey.TestApp.Shared.Constants.Settings.AppSecret).SetFilePath(Windows.Storage.ApplicationData.Current.LocalFolder.Path);
        }
    }
}
