using Newtonsoft.Json;
using RediSharp.Lua;
using RediSharp.RedIL.Resolving.Attributes;
using RediSharp.RedIL.Resolving.CommonResolvers;

namespace RediSharp.Lib
{
    public static class Json
    {
        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.JsonEncode)]
        public static string Encode(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.JsonDecode)]
        public static T Decode<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}