using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.Lib.Internal.Types
{
    class AggregateEnumResolverPack
    {
        private static readonly Dictionary<Aggregate, string> _map = new Dictionary<Aggregate, string>()
        {
            {Aggregate.Sum, "SUM"},
            {Aggregate.Min, "MIN"},
            {Aggregate.Max, "MAX"}
        };

        class EnumResolver : RedILMemberResolver
        {
            private ConstantValueNode _res;

            public EnumResolver(object arg)
            {
                _res = (ConstantValueNode) _map[(Aggregate) arg];
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
                var aggr = (Aggregate) value;
                return (ConstantValueNode) _map[aggr];
            }
        }

        [RedILDataType(DataValueType.String)]
        [RedILResolve(typeof(EnumValueResolver))]
        enum AggregateProxy
        {
            [RedILResolve(typeof(EnumResolver), Aggregate.Sum)]
            Sum,
            [RedILResolve(typeof(EnumResolver), Aggregate.Min)]
            Min,
            [RedILResolve(typeof(EnumResolver), Aggregate.Max)]
            Max
        }

        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(Aggregate), typeof(AggregateProxy)}
            };
        }
    }
}