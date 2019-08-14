using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class StatusNode : ExpressionNode
    {
        public Status Status { get; set; }

        public ExpressionNode Error { get; set; }

        public StatusNode()
            : base(RedILNodeType.Status)
        {
        }

        public StatusNode(Status status, ExpressionNode error = null)
            : base(RedILNodeType.Status)
        {
            Status = status;
            Error = error;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitStatusNode(this, state);
    }
}