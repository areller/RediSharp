using System;
using System.Linq;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp
{
    class CursorRedisMethodResolver : RedILResolver
    {
        private RedisCommand _cmd;
        
        public CursorRedisMethodResolver(object arg)
        {
            _cmd = (RedisCommand) arg;
        }
        
        public override ExpressionNode Resolve(IExpressionVisitor visitor, ExpressionNode caller, ExpressionNode[] arguments)
        {
            // Redis methods expect arguments that are strings, integers, etc... so we have to unpack arrays
            // If we can unpack array in place (if it's an array table definition node), we do it,
            // otherwise, we call the unpack method in Lua
            return new CallRedisMethodNode(_cmd, caller,
                arguments.SelectMany(arg =>
                        arg.DataType == DataValueType.Array
                            ? (arg.Type == RedILNodeType.ArrayTableDefinition
                                ? (arg as ArrayTableDefinitionNode).Elements
                                : WrapSingle(new CallLuaMethodNode(LuaMethod.TableUnpack, new ExpressionNode[] {arg})))
                            : WrapSingle(arg))
                    .ToArray());
        }

        private ExpressionNode[] WrapSingle(ExpressionNode node) => new ExpressionNode[] {node};
    }
}