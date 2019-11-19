using System;
using System.Collections.Generic;
using System.Linq;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.Lib.Internal.Types
{
    class NullableResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                return arguments.First();
            }
        }
        
        class HasValueResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return BinaryExpressionNode.Create(BinaryExpressionOperator.NotEqual, caller, new NilNode());
            }
        }
        
        class ValueResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return caller;
            }
        }
        
        class NullableProxy<T>
            where T : struct
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public NullableProxy(T value)
            {
            }

            [RedILResolve(typeof(ValueResolver))] 
            public T Value { get; set; }

            [RedILResolve(typeof(HasValueResolver))]
            public bool HasValue { get; set; }
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(Nullable<>), typeof(NullableProxy<>) }
            };
        }
    }
}