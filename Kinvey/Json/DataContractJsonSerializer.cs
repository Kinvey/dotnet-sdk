using System;
using System.IO;
using System.Runtime.Serialization;

namespace Kinvey.Json
{
    internal class DataContractJsonSerializer : IJsonSerializer
    {

        private System.Runtime.Serialization.Json.DataContractJsonSerializer GetJsonSerializer(Type type)
        {
            var settings = new System.Runtime.Serialization.Json.DataContractJsonSerializerSettings();
            settings.DateTimeFormat = new DateTimeFormat("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK");
            return new System.Runtime.Serialization.Json.DataContractJsonSerializer(type, settings);
        }

        public object FromJson(Type type, Stream stream)
        {
            return GetJsonSerializer(type).ReadObject(stream);
        }

        public string ToJson(object obj)
        {
            using (var stream = new MemoryStream(4096))
            {
                GetJsonSerializer(obj.GetType()).WriteObject(stream, obj);
                return stream.ToString();
            }
        }
    }
}
