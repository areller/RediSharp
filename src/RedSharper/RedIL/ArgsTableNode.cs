using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class ArgsTableNode : ExpressionNode
    {
        public ArgsTableNode()
            : base(RedILNodeType.ArgsTable, DataValueType.Multi)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitArgsTableNode(this, state);
    }
}