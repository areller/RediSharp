using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL.Nodes
{
    class KeysTableNode : ExpressionNode
    {
        public KeysTableNode()
            : base(RedILNodeType.KeysTable, DataValueType.Array)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitKeysTableNode(this, state);
    }
}