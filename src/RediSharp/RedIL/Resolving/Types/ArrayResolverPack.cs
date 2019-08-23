using System;
using System.Collections.Generic;

namespace RediSharp.RedIL.Resolving.Types
{
    class ArrayResolverPacks
    {
        class ArrayProxy
        {
            
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