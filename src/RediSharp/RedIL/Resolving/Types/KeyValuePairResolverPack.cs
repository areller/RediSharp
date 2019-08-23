using System;
using System.Collections.Generic;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.Resolving.Types
{
    class KeyValuePairResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                throw new NotImplementedException();
            }
        }
        
        public class KeyValuePairProxy<K, V>
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public KeyValuePairProxy(K key, V value)
            {
                
            }

            [RedILResolve(typeof(ConstructorResolver))]
            public KeyValuePairProxy(K key, V value, int[] args)
            {
                
            }

            public K Key { get; }

            public V Value { get; }
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(KeyValuePair<,>), typeof(KeyValuePairProxy<,>) }
            };
        }
    }
}