using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    abstract class RedILNode
    {
        public RedILNode Parent { get; set; }

        public RedILNodeType Type { get; }

        protected RedILNode(RedILNodeType type)
        {
            Type = type;
        }

        public abstract TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state);
    }
}