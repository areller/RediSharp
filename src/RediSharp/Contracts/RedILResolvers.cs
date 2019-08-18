using System.Linq;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.Contracts
{
    class OkStatusResolver : RedILMemberResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return new StatusNode(Status.Ok);
        }
    }

    class ErrorStatusResolver : RedILMethodResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
        {
            return new StatusNode(Status.Error, arguments.FirstOrDefault());
        }
    }

    class StatusIsOkResolver : RedILMemberResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return new BinaryExpressionNode(
                DataValueType.Boolean,
                BinaryExpressionOperator.And,
                new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.NotEqual, caller,
                    new NilNode()),
                new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.NotEqual,
                    new TableKeyAccessNode(caller, new ConstantValueNode(DataValueType.String, "ok")), new NilNode()));
        }
    }

    class StatusMessageResolver : RedILMemberResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return new TableKeyAccessNode(caller, new ConstantValueNode(DataValueType.String, "err"));
        }
    }

    class SingleResultAsIntResolver : RedILMethodResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
        {
            return new CastNode(DataValueType.Integer, caller);
        }
    }

    class SingleResultAsLongResolver : RedILMethodResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
        {
            return new CastNode(DataValueType.Integer, caller);
        }
    }

    class SingleResultAsDoubleResolver : RedILMethodResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
        {
            return new CastNode(DataValueType.Float, caller);
        }
    }
}