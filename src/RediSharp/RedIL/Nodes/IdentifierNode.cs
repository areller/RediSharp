using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class IdentifierNode : ExpressionNode
    {
        public string Name { get; set; }

        public IdentifierNode() : base(RedILNodeType.Parameter) { }

         public IdentifierNode(string name, DataValueType dataType)
            : base(RedILNodeType.Parameter, dataType)
        {
            Name = name;
        }

         public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
             => visitor.VisitIdentifierNode(this, state);
    }
}