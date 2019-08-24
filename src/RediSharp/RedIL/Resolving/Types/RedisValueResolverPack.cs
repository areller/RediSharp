using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class RedisValueResolverPack
    {
        [RedILDataType(DataValueType.String)]
        class RedisValueProxy
        {
            
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(RedisValue), typeof(RedisValueProxy) }
            };
        }
    }
}