using RediSharp.Resolving;
using System.Text.Json;

namespace RediSharp.Lib
{
    public static class Json
    {
        [CallLuaFunction("cjson.encode", new[] { "obj" })]
        public static string Encode(object obj) => JsonSerializer.Serialize(obj);

        [CallLuaFunction("cjson.decode", new[] { "json" })]
        public static T Decode<T>(string json) => JsonSerializer.Deserialize<T>(json);
    }
}