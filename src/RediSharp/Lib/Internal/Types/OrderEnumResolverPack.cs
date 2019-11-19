using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.Lib.Internal.Types
{
    class OrderEnumResolverPack
    {
        private static readonly Dictionary<Order, int> _map = new Dictionary<Order, int>()
        {
            {Order.Ascending, 0},
            {Order.Descending, 1}
        };

        class EnumResolver : RedILMemberResolver
        {
            private ConstantValueNode _res;

            public EnumResolver(object arg)
            {
                _res = (ConstantValueNode) _map[(Order) arg];
            }
            
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return _res;
            }
        }

        class EnumValueResolver : RedILValueResolver
        {
            public override ExpressionNode Resolve(Context context, object value)
            {
                var order = (Order) value;
                return (ConstantValueNode) _map[order];
            }
        }

        [RedILDataType(DataValueType.Integer)]
        [RedILResolve(typeof(EnumValueResolver))]
        enum OrderProxy
        {
            [RedILResolve(typeof(EnumResolver), Order.Ascending)]
            Ascending,
            [RedILResolve(typeof(EnumResolver), Order.Descending)]
            Descending
        }

        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(Order), typeof(OrderProxy)}
            };
        }
    }
}