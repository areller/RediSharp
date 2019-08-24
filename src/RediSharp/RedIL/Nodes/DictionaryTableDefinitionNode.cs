using System.Collections.Generic;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class DictionaryTableDefinitionNode : ExpressionNode
    {
        public IList<KeyValuePair<ExpressionNode, ExpressionNode>> Elements { get; set; }

        public DictionaryTableDefinitionNode()
            : base(RedILNodeType.DictionaryTableDefinition, DataValueType.Dictionary)
        {
            Elements = new List<KeyValuePair<ExpressionNode, ExpressionNode>>();
        }

        public DictionaryTableDefinitionNode(IList<KeyValuePair<ExpressionNode, ExpressionNode>> elements)
            : base(RedILNodeType.DictionaryTableDefinition, DataValueType.Dictionary)
        {
            Elements = elements;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitDictionaryTableDefinition(this, state);
    }
}