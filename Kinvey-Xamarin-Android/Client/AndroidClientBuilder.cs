using System;

namespace Kinvey
{
    public partial class Client
    {
        public static string DefaultFilePath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
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
                Constants.DevicePlatform.Android
            )
            {
            }
        }
    }
}
