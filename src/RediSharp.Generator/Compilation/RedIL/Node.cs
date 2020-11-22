namespace RediSharp.Generator.Compilation.RedIL
{
    abstract class Node
    {
        public Node? Parent { get; }

        protected Node(Node? parent)
        {
            Parent = parent;
        }

        public abstract TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state);
    }
}