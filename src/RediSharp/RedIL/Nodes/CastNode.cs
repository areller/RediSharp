using ICSharpCode.Decompiler.CSharp.Syntax;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
{
    class CastNode : ExpressionNode
    {
        public ExpressionNode Argument { get; set; }

        public CastNode()
            : base(RedILNodeType.Cast)
        {
        }

        public CastNode(DataValueType toType)
            : base(RedILNodeType.Cast, toType)
        {
        }

        public CastNode(DataValueType toType, ExpressionNode arg)
            : base(RedILNodeType.Cast, toType)
        {
            Argument = arg;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCastNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is CastNode)) return false;
            var cast = (CastNode) other;
            return DataType == other.DataType && Argument.EqualOrNull(cast.Argument);
        }

        public override ExpressionNode Simplify() => DataType == Argument?.DataType ? Argument : this;
    }
}