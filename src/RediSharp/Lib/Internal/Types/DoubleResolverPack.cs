using System;
using System.Collections.Generic;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.Lib.Internal.Types
{
    class DoubleResolverPack
    {
        class InfinityResolver : RedILMemberResolver
        {
            private double _inf;

            public InfinityResolver(object arg)
            {
                _inf = (double) arg;
            }
            
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                if (double.IsNegativeInfinity(_inf)) return (ConstantValueNode) "-inf";
                if (double.IsPositiveInfinity(_inf)) return (ConstantValueNode) "+inf";
                throw new NotSupportedException();
            }
        }

        class ValueResolver : RedILValueResolver
        {
            public override ExpressionNode Resolve(Context context, object value)
            {
                var num = (double) value;
                if (double.IsNegativeInfinity(num)) return (ConstantValueNode) "-inf";
                if (double.IsPositiveInfinity(num)) return (ConstantValueNode) "+inf";
                return (ConstantValueNode) num;
            }
        }
        
        [RedILResolve(typeof(ValueResolver))]
        class DoubleProxy
        {
            [RedILResolve(typeof(InfinityResolver), double.NegativeInfinity)]
            public static double NegativeInfinity => default;

            [RedILResolve(typeof(InfinityResolver), double.PositiveInfinity)]
            public static double PositiveInfinity => default;
        }

        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(Double), typeof(DoubleProxy)}
            };
        }
    }
}