using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    sealed class ArgsTableNode : ExpressionNode
    {
        public ArgsTableNode()
            : base(RedILNodeType.ArgsTable, DataValueType.Array)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitArgsTableNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            return other is ArgsTableNode;
        }

        public override ExpressionNode Simplify() => this;
    }
}