using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;
using RediSharp.RedIL.Resolving.CommonResolvers;
using StackExchange.Redis;

namespace RediSharp.Lib.Internal.Types
{
    class SortedSetEntryResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                return new ArrayTableDefinitionNode(new[] {arguments.At(1), arguments.At(0)});
            }
        }
        
        [RedILDataType(DataValueType.KVPair)]
        class SortedSetEntryProxy
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public SortedSetEntryProxy(RedisValue element, double score)
            {}

            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 2)]
            public RedisValue Element => default;

            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 1)]
            public double Score => default;

            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 1)]
            public double Value => default;

            [RedILResolve(typeof(TableAccessMemberResolver), DataValueType.Integer, 2)]
            public RedisValue Key => default;
        }

        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(SortedSetEntry), typeof(SortedSetEntryProxy)}
            };
        }
    }
}