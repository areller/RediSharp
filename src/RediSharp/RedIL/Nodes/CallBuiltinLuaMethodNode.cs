using System.Collections.Generic;
using System.Linq;
using RediSharp.Lua;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
{
    class CallBuiltinLuaMethodNode : ExpressionNode
    {
        private static readonly Dictionary<LuaBuiltinMethod, (DataValueType type, string name)> MethodTypeTable
            = new Dictionary<LuaBuiltinMethod, (DataValueType, string)>()
            {
                {LuaBuiltinMethod.StringSub, (DataValueType.String, "string.sub")},
                {LuaBuiltinMethod.StringToLower, (DataValueType.String, "string.lower")},
                {LuaBuiltinMethod.StringToUpper, (DataValueType.String, "string.upper")},
                {LuaBuiltinMethod.StringLength, (DataValueType.Integer, "string.len")},
                {LuaBuiltinMethod.StringFind, (DataValueType.Integer, "string.find")},
                {LuaBuiltinMethod.StringGSub, (DataValueType.String, "string.gsub")},
                {LuaBuiltinMethod.TableUnpack, (DataValueType.Array, "unpack")},
                {LuaBuiltinMethod.TableInsert, (DataValueType.Unknown, "table.insert")},
                {LuaBuiltinMethod.TableRemove, (DataValueType.Integer, "table.remove")},
                {LuaBuiltinMethod.TableGetN, (DataValueType.Integer, "table.getn")},
                {LuaBuiltinMethod.TableConcat, (DataValueType.String, "table.concat")},
                {LuaBuiltinMethod.Type, (DataValueType.String, "type")},
                {LuaBuiltinMethod.MathAbs, (DataValueType.Float, "math.abs")},
                {LuaBuiltinMethod.MathMin, (DataValueType.Float, "math.min")},
                {LuaBuiltinMethod.MathMax, (DataValueType.Float, "math.max")},
                {LuaBuiltinMethod.JsonEncode, (DataValueType.String, "cjson.encode")},
                {LuaBuiltinMethod.JsonDecode, (DataValueType.Unknown, "cjson.decode")}
            };
        
        public LuaBuiltinMethod Method { get; set; }

        public IList<ExpressionNode> Arguments { get; set; }

        public CallBuiltinLuaMethodNode()
            : base(RedILNodeType.CallLuaMethod)
        {
            Arguments = new List<ExpressionNode>();
        }

        public CallBuiltinLuaMethodNode(
            LuaBuiltinMethod method,
            IList<ExpressionNode> arguments)
            : base(RedILNodeType.CallLuaMethod, MethodTypeTable[method].type)
        {
            Method = method;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallBuiltinLuaMethodNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is CallBuiltinLuaMethodNode)) return false;
            var callLuaMethod = (CallBuiltinLuaMethodNode) other;
            return Method == callLuaMethod.Method && Arguments.AllEqual(callLuaMethod.Arguments);
        }

        public override ExpressionNode Simplify() => new CallBuiltinLuaMethodNode(Method, Arguments.Select(arg => arg.Simplify()).ToList());
    }
}