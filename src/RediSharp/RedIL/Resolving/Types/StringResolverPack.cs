using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.Resolving.Types
{
    class StringResolverPack
    {
        [RedILDataType(DataValueType.String)]
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