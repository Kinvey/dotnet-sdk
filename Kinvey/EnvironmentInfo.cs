using System;
using System.Runtime.Serialization;

namespace Kinvey
{
    [DataContract]
    public class EnvironmentInfo
    {
        [DataMember(Name = "version", IsRequired = true)]
        public string Version { get; private set; }

        [DataMember(Name = "kinvey", IsRequired = true)]
        public string Kinvey { get; private set; }

        [DataMember(Name = "appName", IsRequired = true)]
        public string AppName { get; private set; }

        [DataMember(Name = "environmentName", IsRequired = true)]
        public string EnvironmentName { get; private set; }
    }
}
