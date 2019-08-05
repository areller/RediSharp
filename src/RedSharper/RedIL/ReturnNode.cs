using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class ReturnNode : RedILNode
    {
        public RedILNode Value { get; set; }

        public ReturnNode() : base(RedILNodeType.Return) { }

        public ReturnNode(RedILNode value)
            : base(RedILNodeType.Return)
        {
            Value = value;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}