using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class CastNode : ExpressionNode
    {
        public ExpressionNode Argument { get; set; }

        public CastNode(DataValueType toType)
            : base(RedILNodeType.Cast, toType)
        { }

        public CastNode(DataValueType toType, ExpressionNode arg)
            : base(RedILNodeType.Cast, toType)
        {
            Argument = arg;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCastNode(this, state);
    }
}