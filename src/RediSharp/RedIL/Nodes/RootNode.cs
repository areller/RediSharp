using System.Collections.Generic;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class RootNode : RedILNode
    {
        public IList<VariableDeclareNode> GlobalVariables { get; set; }

        public RedILNode Body { get; set; }

        public HashSet<string> Identifiers { get; set; }

        public RootNode(RedILNode body = null)
            : base(RedILNodeType.Root)
        {
            GlobalVariables = new List<VariableDeclareNode>();
            Body = body;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitRootNode(this, state);
    }
}