using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class ArrayTableDefinitionNode : ExpressionNode
    {
        public ExpressionNode[] Elements { get; set; }

        public ArrayTableDefinitionNode() 
            : base(RedILNodeType.ArrayTableDefinition, DataValueType.Array)
        {
        }

        public ArrayTableDefinitionNode(ExpressionNode[] elements) 
            : base(RedILNodeType.ArrayTableDefinition, DataValueType.Array)
        {
            Elements = elements;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitArrayTableDefinitionNode(this, state);
    }
}