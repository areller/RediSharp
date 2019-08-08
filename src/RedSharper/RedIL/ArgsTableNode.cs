using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class ArgsTableNode : ExpressionNode
    {
        public ArgsTableNode()
            : base(RedILNodeType.ArgsTable, DataValueType.Multi)
        { }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            
        }
    }
}