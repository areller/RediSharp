using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class RedisValueResolverPack
    {
        class IsNullResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return BinaryExpressionNode.Create(BinaryExpressionOperator.Equal, caller, new NilNode());
            }
        }

        class IsNullOrEmptyResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return BinaryExpressionNode.Create(BinaryExpressionOperator.Or,
                    BinaryExpressionNode.Create(BinaryExpressionOperator.Equal, caller, new NilNode()),
                    BinaryExpressionNode.Create(BinaryExpressionOperator.Equal, caller, (ConstantValueNode) ""));
            }
        }

        class HasValueResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return BinaryExpressionNode.Create(BinaryExpressionOperator.And,
                    BinaryExpressionNode.Create(BinaryExpressionOperator.NotEqual, caller, new NilNode()),
                    BinaryExpressionNode.Create(BinaryExpressionOperator.NotEqual, caller, (ConstantValueNode) ""));
            }
        }
        
        [RedILDataType(DataValueType.String)]
        class RedisValueProxy
        {
            [RedILResolve(typeof(HasValueResolver))]
            public bool HasValue { get; }
            
            public bool IsInteger { get; }

            [RedILResolve(typeof(IsNullResolver))]
            public bool IsNull { get; }

            [RedILResolve(typeof(IsNullOrEmptyResolver))]
            public bool IsNullOrEmpty { get; }
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(RedisValue), typeof(RedisValueProxy) }
            };
        }
    }
}