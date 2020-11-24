namespace RediSharp.Generator.Compilation.RedIL
{
    abstract class Node
    {
        public abstract TReturn? AcceptVisitor<TReturn, TState>(RedILVisitor<TReturn, TState> visitor, TState? state)
            where TReturn : class
            where TState : class;

        public TReturn? AcceptVisitor<TVisitor, TReturn, TState>(TState? state)
            where TVisitor : RedILVisitor<TReturn, TState>, new()
            where TReturn : class
            where TState : class
        {
            return AcceptVisitor(new TVisitor(), state);
        }
    }
}