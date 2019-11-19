using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.Lib.Internal.Types
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
                return new CallLuaFunctionNode(LuaFunction.TableCount, DataValueType.Integer, new List<ExpressionNode>() {caller});
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

        class ClearResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallLuaFunctionNode(LuaFunction.TableClear, DataValueType.Unknown, new List<ExpressionNode>() {caller});
            }
        }

        class ContainsKeyResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return BinaryExpressionNode.Create(BinaryExpressionOperator.NotEqual,
                    new TableKeyAccessNode(caller, arguments[0], DataValueType.Unknown), new NilNode());
            }
        }

        class RemoveResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                if (context.IsInsideStatement())
                {
                    return new AssignNode(new TableKeyAccessNode(caller, arguments[0], DataValueType.Unknown), new NilNode());
                }
                else
                {
                    return new CallLuaFunctionNode(LuaFunction.TableDictRemove, DataValueType.Boolean,
                        new List<ExpressionNode>() {caller, arguments[0]});
                }
            }
        }

        class KeysValuesResolver : RedILMemberResolver
        {
            private LuaFunction _func;

            public KeysValuesResolver(object arg)
            {
                _func = (LuaFunction) arg;
            }

            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return new CallLuaFunctionNode(_func, DataValueType.Array,
                    new List<ExpressionNode>() {caller});
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

            [RedILResolve(typeof(ClearResolver))]
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

            [RedILResolve(typeof(ContainsKeyResolver))]
            public bool ContainsKey(K key) => default;

            [RedILResolve(typeof(RemoveResolver))]
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

            [RedILResolve(typeof(KeysValuesResolver), LuaFunction.TableDictKeys)]
            public ICollection<K> Keys { get; }
            
            [RedILResolve(typeof(KeysValuesResolver), LuaFunction.TableDictValues)]
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