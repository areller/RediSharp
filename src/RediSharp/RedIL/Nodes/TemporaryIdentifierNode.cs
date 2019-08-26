using System.Threading;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class TemporaryIdentifierNode : ExpressionNode
    {
        public int Id { get; set; }

        public TemporaryIdentifierNode() 
            : base(RedILNodeType.TemporaryParameter)
        {
        }

        public TemporaryIdentifierNode(DataValueType type)
            : base(RedILNodeType.TemporaryParameter, type)
        {
            Id = Interlocked.Increment(ref _idCount);
        }

        public TemporaryIdentifierNode(int id, DataValueType dataType)
            : base(RedILNodeType.TemporaryParameter, dataType)
        {
            Id = id;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitTemporaryIdentifierNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is TemporaryIdentifierNode)) return false;
            var temp = (TemporaryIdentifierNode)other;
            return Id == temp.Id;
        }

        public override ExpressionNode Simplify() => this;

        #region Static

        private static int _idCount = 0;

        #endregion
    }
}