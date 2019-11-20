using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RediSharp.Lua;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.Lib.Internal.Types
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
        
        class ClearResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallLuaFunctionNode(LuaFunction.TableClear, DataValueType.Unknown, new List<ExpressionNode>() {caller});
            }
        }

        class ContainsResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallLuaFunctionNode(LuaFunction.TableArrayContains, DataValueType.Boolean,
                    new List<ExpressionNode>() {caller, arguments[0]});
            }
        }
        
        class RemoveResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallLuaFunctionNode(LuaFunction.TableArrayRemove, DataValueType.Boolean,
                    new List<ExpressionNode>() {caller, arguments[0]});
            }
        }

        class IndexOfResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallLuaFunctionNode(LuaFunction.TableArrayIndexOf, DataValueType.Integer,
                    new List<ExpressionNode>() {caller, arguments[0]});
            }
        }

        class InsertResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.TableInsert, new List<ExpressionNode>()
                {
                    caller, (ConstantValueNode)1 + arguments[0], arguments[1]
                });
            }
        }

        class RemoveAtResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.TableRemove, new List<ExpressionNode>()
                {
                    caller, (ConstantValueNode) 1 + arguments[0]
                });
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

            [RedILResolve(typeof(ClearResolver))]
            public void Clear()
            {
            }

            [RedILResolve(typeof(ContainsResolver))]
            public bool Contains(T item) => default;

            public void CopyTo(T[] array, int arrayIndex)
            {
            }

            [RedILResolve(typeof(RemoveResolver))]
            public bool Remove(T item) => default;

            [RedILResolve(typeof(CountResolver))]
            public int Count { get; }
            public bool IsReadOnly { get; }

            [RedILResolve(typeof(IndexOfResolver))]
            public int IndexOf(T item) => default;
            
            [RedILResolve(typeof(InsertResolver))]
            public void Insert(int index, T item)
            {
            }

            [RedILResolve(typeof(RemoveAtResolver))]
            public void RemoveAt(int index)
            {
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