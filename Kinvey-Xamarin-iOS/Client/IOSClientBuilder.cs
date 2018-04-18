using Foundation;
using SQLite.Net.Platform.XamarinIOS;

namespace Kinvey
{
    public class IOSClientBuilder : Client.Builder
    {
        public IOSClientBuilder(string appKey, string appSecret) :
        base(appKey, appSecret, Constants.DevicePlatform.iOS)
        {
            this.setFilePath(NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].ToString());
            this.setOfflinePlatform(new SQLitePlatformIOS());
        }
    }
}
