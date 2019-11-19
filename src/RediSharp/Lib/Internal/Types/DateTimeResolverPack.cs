using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.Lib.Internal.Types
{
    class DateTimeResolverPack
    {
        [RedILDataType(DataValueType.Float)]
        class DateTimeProxy
        {
            
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(DateTime), typeof(DateTimeProxy) }
            };
        }
    }
}