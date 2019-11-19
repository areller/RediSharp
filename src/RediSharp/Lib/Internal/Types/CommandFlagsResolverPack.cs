using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.Lib.Internal.Types
{
    class CommandFlagsResolverPack
    {
        [RedILDataType(DataValueType.Integer)]
        enum CommandFlagsProxy
        {
            
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(CommandFlags), typeof(CommandFlagsProxy)}
            };
        }
    }
}