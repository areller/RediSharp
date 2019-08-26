using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

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

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is StatusNode)) return false;
            var status = (StatusNode) other;
            return Status == status.Status &&
                   Error.EqualOrNull(status.Error);
        }

        public override ExpressionNode Simplify() => new StatusNode(Status, Error.Simplify());
    }
}