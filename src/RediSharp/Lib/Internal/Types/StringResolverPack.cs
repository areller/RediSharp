using System;
using System.Collections.Generic;
using System.Linq;
using RediSharp.Lua;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;
using RediSharp.RedIL.Resolving.CommonResolvers;

namespace RediSharp.Lib.Internal.Types
{
    class StringResolverPack
    {
        class SubstringResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                switch (arguments.Length)
                {
                    case 1:
                        return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.StringSub,
                            new List<ExpressionNode>() {caller, arguments[0] + (ConstantValueNode) 1});
                    case 2:
                        return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.StringSub,
                            new List<ExpressionNode>()
                                {caller, arguments[0] + (ConstantValueNode) 1, arguments[0] + arguments[1]});
                    default: return null;
                }
            }
        }

        class ContainsResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return BinaryExpressionNode.Create(BinaryExpressionOperator.NotEqual,
                    new CallBuiltinLuaMethodNode(LuaBuiltinMethod.StringFind,
                        new List<ExpressionNode>() {caller, arguments[0]}), new NilNode());
            }
        }

        class ReplaceResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.StringGSub,
                    new List<ExpressionNode>() {caller, arguments[0], arguments[1]});
            }
        }

        class IndexOfResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                switch (arguments.Length)
                {
                    case 1:
                        return (new CallBuiltinLuaMethodNode(LuaBuiltinMethod.StringFind,
                                   new List<ExpressionNode>() {caller, arguments[0]})) - (ConstantValueNode) 1;
                    case 2:
                        return (new CallBuiltinLuaMethodNode(LuaBuiltinMethod.StringFind,
                                   new List<ExpressionNode>()
                                       {caller, arguments[0], arguments[1] + (ConstantValueNode) 1})) -
                               (ConstantValueNode) 1;
                    default: return null;
                } 
            }
        }

        class JoinResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new CallBuiltinLuaMethodNode(LuaBuiltinMethod.TableConcat,
                    new List<ExpressionNode>()
                    {
                        arguments.Length == 2 && arguments[1].DataType == DataValueType.Array
                            ? arguments[1]
                            : new ArrayTableDefinitionNode(arguments.Skip(1).ToList()),
                        arguments[0]
                    });
            }
        }
        
        [RedILDataType(DataValueType.String)]
        class StringProxy
        {
            [RedILResolve(typeof(CallLuaBuiltinMemberResolver), LuaBuiltinMethod.StringLength)]
            public int Length { get; }

            [RedILResolve(typeof(CallLuaBuiltinMethodResolver), LuaBuiltinMethod.StringToLower)]
            public string ToLower() => default;

            [RedILResolve(typeof(CallLuaBuiltinMethodResolver), LuaBuiltinMethod.StringToUpper)]
            public string ToUpper() => default;

            [RedILResolve(typeof(SubstringResolver))]
            public string Substring(int start) => default;

            [RedILResolve(typeof(SubstringResolver))]
            public string Substring(int start, int length) => default;

            [RedILResolve(typeof(ContainsResolver))]
            public bool Contains(string substr) => default;

            [RedILResolve(typeof(ReplaceResolver))]
            public string Replace(char oldChar, char newChar) => default;

            [RedILResolve(typeof(ReplaceResolver))]
            public string Replace(string oldStr, string newStr) => default;

            [RedILResolve(typeof(IndexOfResolver))]
            public int IndexOf(char ch) => default;

            [RedILResolve(typeof(IndexOfResolver))]
            public int IndexOf(string str) => default;

            [RedILResolve(typeof(IndexOfResolver))]
            public int IndexOf(char ch, int startIndex) => default;

            [RedILResolve(typeof(IndexOfResolver))]
            public int IndexOf(string str, int startIndex) => default;

            [RedILResolve(typeof(JoinResolver))]
            public static string Join(string separator, params string[] parts) => default;

            [RedILResolve(typeof(JoinResolver))]
            public static string Join(string separator, IEnumerable<string> parts) => default;

            [RedILResolve(typeof(JoinResolver))]
            public static string Join(char separator, params string[] parts) => default;

            [RedILResolve(typeof(JoinResolver))]
            public static string Join(char separator, IEnumerable<string> parts) => default;

            [RedILResolve(typeof(JoinResolver))]
            public static string Join(string separator, params object[] parts) => default;

            [RedILResolve(typeof(JoinResolver))]
            public static string Join(char separator, params object[] parts) => default;
        }
        
        public static Dictionary<Type, Type> GetMapToProxy()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(string), typeof(StringProxy) }
            };
        }
    }
}