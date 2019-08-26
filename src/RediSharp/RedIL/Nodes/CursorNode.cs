using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class CursorNode : ExpressionNode
    {
        public CursorNode()
            : base(RedILNodeType.Cursor, DataValueType.Unknown)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCursorNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            return other is CursorNode;
        }

        public override ExpressionNode Simplify() => this;
    }
}