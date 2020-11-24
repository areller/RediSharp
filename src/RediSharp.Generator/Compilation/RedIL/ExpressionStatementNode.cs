using RediSharp.Generator.Compilation.RedIL.Expressions;

namespace RediSharp.Generator.Compilation.RedIL
{
    sealed class ExpressionStatementNode : Node
    {
        public ExpressionNode Expression { get; }

        public ExpressionStatementNode(ExpressionNode expression)
        {
            Expression = expression;
        }

        public override TReturn? AcceptVisitor<TReturn, TState>(RedILVisitor<TReturn, TState> visitor, TState? state)
            where TReturn : class
            where TState : class
        {
            return visitor.VisitExpressionStatementNode(this, state);
        }
    }
}
