using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;
using RediSharp.RedIL.Resolving.CommonResolvers;

namespace RediSharp.Lib.Internal.Types
{
    class KeyValuePairResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments,
                ExpressionNode[] elements)
            {
                return new ArrayTableDefinitionNode(new ExpressionNode[] {arguments.At(0), arguments.At(1)});
            }
        }
        
        [RedILDataType(DataValueType.KVPair)]
        public class KeyValuePairProxy<K, V>
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public KeyValuePairProxy(K key, V value)
            {
                
            }

            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 1)]
            public K Key { get; }

            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 2)]
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