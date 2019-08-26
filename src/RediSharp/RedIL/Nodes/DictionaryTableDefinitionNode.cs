using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.Semantics;
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

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is DictionaryTableDefinitionNode)) return false;
            var dict = (DictionaryTableDefinitionNode) other;
            if (Elements is null && dict.Elements is null)
                return true;
            else if ((Elements is null && !(dict.Elements is null)) || (!(Elements is null) && dict.Elements is null))
                return false;
            else
                return Elements.SequenceEqual(dict.Elements,
                    new KeyValuePairEqualityComparer<ExpressionNode, ExpressionNode>());
        }

        public override ExpressionNode Simplify() => this;
    }
}