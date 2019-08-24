using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.Resolving.Types
{
    class ArrayResolverPacks
    {
        [RedILDataType(DataValueType.Array)]
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