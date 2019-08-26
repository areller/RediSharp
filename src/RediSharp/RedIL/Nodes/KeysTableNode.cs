using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class KeysTableNode : ExpressionNode
    {
        public KeysTableNode()
            : base(RedILNodeType.KeysTable, DataValueType.Array)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitKeysTableNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            return other is KeysTableNode;
        }

        public override ExpressionNode Simplify() => this;
    }
}