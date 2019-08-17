using System.Collections.Generic;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class ArrayTableDefinitionNode : ExpressionNode
    {
        public IList<ExpressionNode> Elements { get; set; }

        public ArrayTableDefinitionNode() 
            : base(RedILNodeType.ArrayTableDefinition, DataValueType.Array)
        {
            Elements = new List<ExpressionNode>();
        }

        public ArrayTableDefinitionNode(IList<ExpressionNode> elements) 
            : base(RedILNodeType.ArrayTableDefinition, DataValueType.Array)
        {
            Elements = elements;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitArrayTableDefinitionNode(this, state);
    }
}