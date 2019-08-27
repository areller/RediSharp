using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving.CommonResolvers
{
    class TableAccessMemberResolver : RedILMemberResolver
    {
        private DataValueType _dataType;
        
        private object _key;

        public TableAccessMemberResolver(object arg1, object arg2)
        {
            _dataType = (DataValueType) arg1;
            _key = arg2;
        }
        
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return new TableKeyAccessNode(caller, new ConstantValueNode(_dataType, _key), context.Compiler.ResolveExpressionType(context.CurrentExpression));
        }
    }
}