using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class RedisValueResolverPack
    {
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