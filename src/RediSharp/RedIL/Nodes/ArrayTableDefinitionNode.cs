using System.Collections.Generic;
using System.Linq;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

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

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is ArrayTableDefinitionNode)) return false;
            var arrayDef = (ArrayTableDefinitionNode) other;
            return Elements.AllEqual(arrayDef.Elements);
        }

        public override ExpressionNode Simplify() => this;
    }
}