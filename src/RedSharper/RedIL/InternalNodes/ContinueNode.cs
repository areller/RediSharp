using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL.InternalNodes
{
    class ContinueNode : RedILNode
    {
        public ContinueNode() 
            : base(RedILNodeType.Continue)
        {
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
        {
            // We don't want to visit continue nodes outside of RedIL's internal scope
            throw new System.NotImplementedException();
        }
    }
}