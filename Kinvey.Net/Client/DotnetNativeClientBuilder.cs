using SQLite.Net.Platform.Win32;

namespace Kinvey
{
    public class DotnetNativeClientBuilder : Client.Builder
    {
        public DotnetNativeClientBuilder(string appKey, string appSecret) :
        base(appKey, appSecret, Constants.DevicePlatform.NET)
        {
            this.setOfflinePlatform(new SQLitePlatformWin32());
        }
    }
}
