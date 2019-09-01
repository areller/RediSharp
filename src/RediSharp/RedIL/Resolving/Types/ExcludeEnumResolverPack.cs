using System;
using System.Collections.Generic;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.RedIL.Resolving.Types
{
    class ExcludeEnumResolverPack
    {
        private static readonly Dictionary<Exclude, int> _map = new Dictionary<Exclude, int>()
        {
            {Exclude.None, 0},
            {Exclude.Start, 1},
            {Exclude.Stop, 2},
            {Exclude.Both, 3}
        };

        class EnumResolver : RedILMemberResolver
        {
            private ConstantValueNode _res;

            public EnumResolver(object arg)
            {
                _res = (ConstantValueNode) _map[(Exclude) arg];
            }
            
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return _res;
            }
        }

        class EnumValueResolver : RedILValueResolver
        {
            public override ExpressionNode Resolve(Context context, object value)
            {
                var exclude = (Exclude) value;
                return (ConstantValueNode) _map[exclude];
            }
        }

        [RedILDataType(DataValueType.Integer)]
        [RedILResolve(typeof(EnumValueResolver))]
        enum ExcludeProxy
        {
            [RedILResolve(typeof(EnumResolver), Exclude.None)]
            None,
            [RedILResolve(typeof(EnumResolver), Exclude.Start)]
            Start,
            [RedILResolve(typeof(EnumResolver), Exclude.Stop)]
            Stop,
            [RedILResolve(typeof(EnumResolver), Exclude.Both)]
            Both
        }

        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                {typeof(Exclude), typeof(ExcludeProxy)}
            };
        }
    }
}