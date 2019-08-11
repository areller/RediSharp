using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class KeysTableNode : ExpressionNode
    {
        public KeysTableNode()
            : base(RedILNodeType.KeysTable, DataValueType.Multi)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitKeysTableNode(this, state);
    }
}