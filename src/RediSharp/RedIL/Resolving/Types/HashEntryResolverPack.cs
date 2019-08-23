using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class HashEntryResolverPack
    {
        class HashEntryProxy
        {
            
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(HashEntry), typeof(HashEntryProxy) }
            };
        }
    }
}