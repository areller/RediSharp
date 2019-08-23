using System;
using System.Collections.Generic;

namespace RediSharp.RedIL.Resolving.Types
{
    class DateTimeResolverPack
    {
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