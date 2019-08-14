using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class ArgsTableNode : ExpressionNode
    {
        public ArgsTableNode()
            : base(RedILNodeType.ArgsTable, DataValueType.Array)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitArgsTableNode(this, state);
    }
}