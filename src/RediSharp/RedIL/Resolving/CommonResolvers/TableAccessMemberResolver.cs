using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving.CommonResolvers
{
    class TableAccessMemberResolver : RedILMemberResolver
    {
        private string _key;

        public TableAccessMemberResolver(object arg)
        {
            _key = (string) arg;
        }
        
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return new TableKeyAccessNode(caller, (ConstantValueNode) _key, context.Compiler.ResolveExpressionType(context.CurrentExpression));
        }
    }
}