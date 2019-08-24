using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;

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

        class KVAccessResolver : RedILMemberResolver
        {
            private string _key;

            public KVAccessResolver(object arg)
            {
                _key = (string) arg;
            }
            
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return new TableKeyAccessNode(caller, (ConstantValueNode) _key, context.Compiler.ResolveExpressionType(context.CurrentExpression));
            }
        }
        
        [RedILDataType(DataValueType.KVPair)]
        public class KeyValuePairProxy<K, V>
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public KeyValuePairProxy(K key, V value)
            {
                
            }

            [RedILResolve(typeof(KVAccessResolver), "key")]
            public K Key { get; }

            [RedILResolve(typeof(KVAccessResolver), "value")]
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