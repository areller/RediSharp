using RediSharp.Generator.Compilation.RedIL.Expressions;

namespace RediSharp.Generator.Compilation.RedIL
{
    interface IRedILVisitor<TReturn, TState>
    {
        TReturn VisitBlockNode(BlockNode blockNode, TState state);

        TReturn VisitConstantExpressionNode(ConstantExpressionNode constantExpressionNode, TState state);
    }
}