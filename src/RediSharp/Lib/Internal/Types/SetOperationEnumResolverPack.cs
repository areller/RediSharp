using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.Lib.Internal.Types
{
    class SetOperationEnumResolverPack
    {
        private static readonly Dictionary<SetOperation, int> _map = new Dictionary<SetOperation, int>()
        {
            {SetOperation.Union, 0},
            {SetOperation.Intersect, 1},
            {SetOperation.Difference, 2}
        };

        class EnumResolver : RedILMemberResolver
        {
            private ConstantValueNode _res;

            public EnumResolver(object arg)
            {
                _res = (ConstantValueNode) _map[(SetOperation) arg];
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
                var setOp = (SetOperation) value;
                return (ConstantValueNode) _map[setOp];
            }
        }

        [RedILDataType(DataValueType.Integer)]
        [RedILResolve(typeof(EnumValueResolver))]
        enum SetOperationProxy
        {
            [RedILResolve(typeof(EnumResolver), SetOperation.Union)]
            Union,
            [RedILResolve(typeof(EnumResolver), SetOperation.Intersect)]
            Intersect,
            [RedILResolve(typeof(EnumResolver), SetOperation.Difference)]
            Difference
        }

        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(SetOperation), typeof(SetOperationProxy)}
            };
        }
    }
}