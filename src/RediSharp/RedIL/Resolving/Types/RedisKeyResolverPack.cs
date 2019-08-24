using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class RedisKeyResolverPack
    {
        [RedILDataType(DataValueType.String)]
        class RedisKeyProxy
        {
            
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(RedisKey), typeof(RedisKeyProxy) }
            };
        }
    }
}