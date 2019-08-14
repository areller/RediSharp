using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Attributes
{
    abstract class RedILResolver
    {
        class RedILResolverVisitor : IExpressionVisitor
        {
            public ExpressionNode Visit(ExpressionNode node)
            {
                throw new System.NotImplementedException();
            }
        }
        
        public abstract ExpressionNode Resolve(IExpressionVisitor visitor, ExpressionNode caller, ExpressionNode[] arguments);
    }
}