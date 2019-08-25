using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;
using RediSharp.RedIL.Resolving.CommonResolvers;

namespace RediSharp.RedIL.Resolving.Types
{
    class KeyValuePairResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments,
                ExpressionNode[] elements)
            {
                return new DictionaryTableDefinitionNode(new List<KeyValuePair<ExpressionNode, ExpressionNode>>()
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) "key", arguments[0]),
                    new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) "value", arguments[1])
                });
            }
        }
        
        [RedILDataType(DataValueType.KVPair)]
        public class KeyValuePairProxy<K, V>
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public KeyValuePairProxy(K key, V value)
            {
                
            }

            [RedILResolve(typeof(TableAccessMemberResolver), "key")]
            public K Key { get; }

            [RedILResolve(typeof(TableAccessMemberResolver), "value")]
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