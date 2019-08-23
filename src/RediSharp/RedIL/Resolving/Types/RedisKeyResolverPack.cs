using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class RedisKeyResolverPack
    {
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