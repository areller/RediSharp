using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class WhenEnumResolverPack
    {
        class EnumResolver : RedILMemberResolver
        {
            private ConstantValueNode _res;

            private static readonly Dictionary<When, int> _map = new Dictionary<When, int>()
            {
                {When.Always, 0},
                {When.Exists, 1},
                {When.NotExists, 2}
            };
            
            public EnumResolver(object arg)
            {
                _res = (ConstantValueNode) _map[(When) arg];
            }

            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return _res;
            }
        }
        
        [RedILDataType(DataValueType.Integer)]
        enum WhenProxy
        {
            [RedILResolve(typeof(EnumResolver), When.Always)]
            Always,
            [RedILResolve(typeof(EnumResolver), When.Exists)]
            Exists,
            [RedILResolve(typeof(EnumResolver), When.NotExists)]
            NotExists
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(When), typeof(WhenProxy)}
            };
        }
    }
}