using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.Resolving.Types
{
    class DictionaryResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                return new DictionaryTableDefinitionNode(elements.Select(e => e as ArrayTableDefinitionNode)
                    .Select(e => new KeyValuePair<ExpressionNode, ExpressionNode>(e.Elements[0], e.Elements[1]))
                    .ToList());
            }
        }

        class CountResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return new CallLuaFunctionNode("count_tbl", DataValueType.Integer, new List<ExpressionNode>() {caller});
            }
        }

        class AddResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                if (arguments.Length == 2)
                {
                    return new AssignNode(new TableKeyAccessNode(caller, arguments[0], DataValueType.Unknown), arguments[1]);
                }
                else
                {
                    var arg = arguments[0] as DictionaryTableDefinitionNode;
                    return new AssignNode(new TableKeyAccessNode(caller, arg.Elements[0].Value, DataValueType.Unknown), arg.Elements[1].Value);
                }
            }
        }
        
        [RedILDataType(DataValueType.Dictionary)]
        class DictionaryProxy<K, V> : IDictionary<K,V>
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public DictionaryProxy()
            {
            }

            public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => default;

            IEnumerator IEnumerable.GetEnumerator() => default;

            [RedILResolve(typeof(AddResolver))]
            public void Add(KeyValuePair<K, V> item)
            {
            }

            public void Clear()
            {
            }

            public bool Contains(KeyValuePair<K, V> item) => default;

            public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
            {
            }

            public bool Remove(KeyValuePair<K, V> item) => default;

            [RedILResolve(typeof(CountResolver))]
            public int Count { get; }
            public bool IsReadOnly { get; }

            [RedILResolve(typeof(AddResolver))]
            public void Add(K key, V value)
            {
            }

            public bool ContainsKey(K key) => default;

            public bool Remove(K key) => default;

            public bool TryGetValue(K key, out V value)
            {
                throw new NotImplementedException();
            }

            public V this[K key]
            {
                get => default;
                set => throw new NotImplementedException();
            }

            public ICollection<K> Keys { get; }
            public ICollection<V> Values { get; }
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(Dictionary<,>), typeof(DictionaryProxy<,>) }
            };
        }
    }
}