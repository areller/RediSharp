using System;
using System.Collections.Generic;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.Resolving.Types
{
    class NullableResolverPack
    {
        class HasValueResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                throw new NotImplementedException();
            }
        }
        
        class ValueResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                throw new NotImplementedException();
            }
        }
        
        class NullableProxy<T>
            where T : struct
        {
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