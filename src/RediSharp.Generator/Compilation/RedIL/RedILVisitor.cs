using RediSharp.Generator.Compilation.RedIL.Expressions;

namespace RediSharp.Generator.Compilation.RedIL
{
    abstract class RedILVisitor<TReturn, TState>
        where TReturn : class
        where TState : class
    {
        public virtual TReturn? DefaultVisit(Node node, TState? state) { return default; }

        public virtual TReturn? DefaultExpressionVisit(ExpressionNode expressionNode, TState? state) => DefaultVisit(expressionNode, state);

        public virtual TReturn? VisitBlockNode(BlockNode blockNode, TState? state) => DefaultVisit(blockNode, state);

        public virtual TReturn? VisitExpressionStatementNode(ExpressionStatementNode expressionStatementNode, TState? state) => DefaultVisit(expressionStatementNode, state);

        public virtual TReturn? VisitConstantExpressionNode(ConstantExpressionNode constantExpressionNode, TState? state) => DefaultExpressionVisit(constantExpressionNode, state);
    }
}