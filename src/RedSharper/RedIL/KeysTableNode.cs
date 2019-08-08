using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class KeysTableNode : ExpressionNode
    {
        public KeysTableNode()
            : base(RedILNodeType.KeysTable, DataValueType.Multi)
        { }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}