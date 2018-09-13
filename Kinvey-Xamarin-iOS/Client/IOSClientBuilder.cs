using Foundation;

namespace Kinvey
{
    public partial class Client
    {

        public static string DefaultFilePath
        {
            get
            {
                return NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].ToString();
            }
        }

        public partial class Builder
        {
            public Builder(
                string appKey,
                string appSecret
            ) : this(
                appKey,
                appSecret,
                DefaultFilePath,
                Constants.DevicePlatform.iOS
            )
            {
            }
        }
    }
}
