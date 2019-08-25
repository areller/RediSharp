using System;
using System.Collections.Generic;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Resolving.Attributes;
using RediSharp.RedIL.Resolving.CommonResolvers;

namespace RediSharp.RedIL.Resolving.Types
{
    class ArrayResolverPacks
    {
        [RedILDataType(DataValueType.Array)]
        class ArrayProxy
        {
            [RedILResolve(typeof(CallLuaBuiltinMemberResolver), LuaBuiltinMethod.TableGetN)]
            public int Length { get; }
        }

        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(Array), typeof(ArrayProxy) }
            };
        }
    }
}