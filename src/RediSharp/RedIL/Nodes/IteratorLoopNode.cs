using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class IteratorLoopNode : RedILNode
    {
        public DataValueType CursorType { get; set; }

        public string CursorName { get; set; }

        public ExpressionNode Over { get; set; }

        public BlockNode Body { get; set; }

        public IteratorLoopNode()
            : base(RedILNodeType.IteratorLoop)
        {
        }

        public IteratorLoopNode(
            DataValueType cursorType,
            string cursorName,
            ExpressionNode over,
            BlockNode body)
            : base(RedILNodeType.IteratorLoop)
        {
            CursorType = cursorType;
            CursorName = cursorName;
            Over = over;
            Body = body;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitIteratorLoopNode(this, state);
    }
}