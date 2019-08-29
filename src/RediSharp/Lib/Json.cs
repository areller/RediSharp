using RediSharp.Enums;
using RediSharp.RedIL.Resolving.Attributes;
using RediSharp.RedIL.Resolving.CommonResolvers;

namespace RediSharp.Lib
{
    public static class Json
    {
        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.JsonEncode)]
        public static string Encode(object obj)
        {
            //TODO: Use Newtonsoft.Json for debugging
            return string.Empty;
        }

        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.JsonDecode)]
        public static T Decode<T>(string json)
        {
            //TODO: Use Newtonsoft.Json for debugging
            return default;
        }
    }
}