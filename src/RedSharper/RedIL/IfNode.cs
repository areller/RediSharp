using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class IfNode : RedILNode
    {
        public RedILNode Condition { get; set; }

        public RedILNode IfTrue { get; set; }

        public RedILNode IfFalse { get; set; }

        public IfNode() : base(RedILNodeType.If) { }

        public IfNode(
            RedILNode condition,
            RedILNode ifTrue,
            RedILNode ifFalse)
            : base(RedILNodeType.If)
        {
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitIfNode(this, state);
    }
}