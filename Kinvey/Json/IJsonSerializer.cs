using System;
using System.IO;

namespace Kinvey.Json
{
    public interface IJsonSerializer
    {
        object FromJson(Type type, Stream stream);
        string ToJson(object obj);
    }
}
