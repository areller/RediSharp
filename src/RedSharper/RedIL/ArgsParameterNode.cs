namespace RedSharper.RedIL
{
    class ArgsParameterNode : ParameterNode
    {
        const string ARGS = "ARGV";

        public ArgsParameterNode()
            : base(ARGS)
        { }
    }
}