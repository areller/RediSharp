using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.Resolving.Types
{
    class ListResolverPack
    {
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                return new ArrayTableDefinitionNode(elements.ToList());
            }
        }

        class AddResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.TableInsert,
                    new List<ExpressionNode>() {caller, arguments[0]});
            }
        }

        class CountResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.TableGetN, new List<ExpressionNode>() {caller});
            }
        }
        
        [RedILDataType(DataValueType.Array)]
        class ListProxy<T> : IList<T>
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public ListProxy()
            {
            }

            public IEnumerator<T> GetEnumerator() => default;

            IEnumerator IEnumerable.GetEnumerator() => default;

            [RedILResolve(typeof(AddResolver))]
            public void Add(T item)
            {
            }

            public void Clear()
            {
                throw new System.NotImplementedException();
            }

            public bool Contains(T item)
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            public bool Remove(T item)
            {
                throw new System.NotImplementedException();
            }

            [RedILResolve(typeof(CountResolver))]
            public int Count { get; }
            public bool IsReadOnly { get; }

            public int IndexOf(T item)
            {
                throw new System.NotImplementedException();
            }

            public void Insert(int index, T item)
            {
                throw new System.NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new System.NotImplementedException();
            }

            public T this[int index]
            {
                get => throw new System.NotImplementedException();
                set => throw new System.NotImplementedException();
            }
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(List<>), typeof(ListProxy<>) },
                { typeof(IList<>), typeof(ListProxy<>) },
                { typeof(ICollection<>), typeof(ListProxy<>) }
            };
        }
    }
}