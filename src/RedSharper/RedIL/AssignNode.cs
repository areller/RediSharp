using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class AssignNode : RedILNode
    {
        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public AssignNode() : base(RedILNodeType.Assign) { }

        public AssignNode(
            ExpressionNode left,
            ExpressionNode right)
            : base(RedILNodeType.Assign)
        {
            Left = left;
            Right = right;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            visitor.VisitAssignNode(this, state);
        }
    }
}