using System;
using System.Linq;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp
{
    class CursorRedisMethodResolver : RedILMethodResolver
    {
        private string _cmd;

        private DataValueType _returnType;
        
        public CursorRedisMethodResolver(object arg1, object arg2)
        {
            _cmd = (string) arg1;
            _returnType = (DataValueType) arg2;
        }
        
        public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
        {
            // Redis methods expect arguments that are strings, integers, etc... so we have to unpack arrays
            // If we can unpack array in place (if it's an array table definition node), we do it,
            // otherwise, we call the unpack method in Lua
            return new CallRedisMethodNode(_cmd, _returnType, caller,
                arguments.SelectMany(arg =>
                        arg.DataType == DataValueType.Array
                            ? (arg.Type == RedILNodeType.ArrayTableDefinition
                                ? (arg as ArrayTableDefinitionNode).Elements
                                : WrapSingle(new CallBuiltinLuaMethodNode(LuaBuiltinMethod.TableUnpack, new ExpressionNode[] {arg})))
                            : WrapSingle(arg))
                    .ToArray());
        }

        private ExpressionNode[] WrapSingle(ExpressionNode node) => new ExpressionNode[] {node};
    }
}