using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class ParameterNode : RedILNode
    {
        public string Name { get; set; }

        public ParameterNode() : base(RedILNodeType.Parameter) { }

         public ParameterNode(string name)
            : base(RedILNodeType.Parameter)
        {
            Name = name;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}