using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;
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
                return new ArrayTableDefinitionNode(new ExpressionNode[] {arguments.At(0), arguments.At(1)});
            }
        }
        
        [RedILDataType(DataValueType.KVPair)]
        class HashEntryProxy
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public HashEntryProxy(RedisValue name, RedisValue value)
            {
            }
            
            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 1)]
            public RedisValue Name { get; }

            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 2)]
            public RedisValue Value { get; }

            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 1)]
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