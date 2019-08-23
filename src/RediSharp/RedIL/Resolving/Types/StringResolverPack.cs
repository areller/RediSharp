using System;
using System.Collections.Generic;

namespace RediSharp.RedIL.Resolving.Types
{
    class StringResolverPack
    {
        class StringProxy
        {
            
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(string), typeof(StringProxy) }
            };
        }
    }
}