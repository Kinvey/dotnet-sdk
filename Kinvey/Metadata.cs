using System;
using System.Runtime.Serialization;

namespace Kinvey
{
    [DataContract]
    public class Metadata
    {
        [DataMember(Name = "lmt", IsRequired = true)]
        public DateTime LastModifiedTime;

        [DataMember(Name = "ect", IsRequired = true)]
        public DateTime EntityCreationTime;
    }
}
