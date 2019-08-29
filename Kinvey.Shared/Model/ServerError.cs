using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinvey
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerError
    {
        [JsonProperty]
        internal string Error { get; set; }

        [JsonProperty]
        internal string Description { get; set; }

        [JsonProperty]
        internal string Debug { get; set; }
    }
}
