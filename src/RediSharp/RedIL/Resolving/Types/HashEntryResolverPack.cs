using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;
using RediSharp.RedIL.Resolving.CommonResolvers;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class HashEntryResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                return new DictionaryTableDefinitionNode(new List<KeyValuePair<ExpressionNode, ExpressionNode>>()
                {
                    new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) "key", arguments[0]),
                    new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode) "value", arguments[1])
                });
            }
        }
        
        [RedILDataType(DataValueType.KVPair)]
        class HashEntryProxy
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public HashEntryProxy(RedisValue name, RedisValue value)
            {
            }
            
            [RedILResolve(typeof(TableAccessMemberResolver), "key")]
            public RedisValue Name { get; }

            [RedILResolve(typeof(TableAccessMemberResolver), "value")]
            public RedisValue Value { get; }

            [RedILResolve(typeof(TableAccessMemberResolver), "key")]
            public RedisValue Key { get; }
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(HashEntry), typeof(HashEntryProxy) }
            };
        }
    }
}