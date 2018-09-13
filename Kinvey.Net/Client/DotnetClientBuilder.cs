using System;

namespace Kinvey
{
    public partial class Client
    {
        public partial class Builder
        {
            public Builder(
                string appKey,
                string appSecret
            ) : this(
                appKey,
                appSecret,
                Environment.CurrentDirectory,
                Constants.DevicePlatform.NET
            )
            {
            }
        }
    }
}
