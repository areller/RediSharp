using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class AssignNode : RedILNode
    {
        public RedILNode Left { get; set; }

        public RedILNode Right { get; set; }

        public AssignNode() : base(RedILNodeType.Assign) { }

        public AssignNode(
            RedILNode left,
            RedILNode right)
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