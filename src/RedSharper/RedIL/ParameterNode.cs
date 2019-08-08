using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class ParameterNode : ExpressionNode
    {
        public string Name { get; set; }

        public ParameterNode() : base(RedILNodeType.Parameter) { }

         public ParameterNode(string name, DataValueType dataType)
            : base(RedILNodeType.Parameter, dataType)
        {
            Name = name;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}