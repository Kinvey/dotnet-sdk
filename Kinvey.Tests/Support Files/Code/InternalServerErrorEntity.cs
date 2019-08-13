using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinvey.Tests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InternalServerErrorEntity : Entity
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
