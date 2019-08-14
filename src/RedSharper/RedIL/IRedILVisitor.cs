using ICSharpCode.Decompiler.Util;
using RedSharper.RedIL.Nodes;

namespace RedSharper.RedIL
{
    interface IRedILVisitor<TReturn, TState>
    {
        TReturn VisitArgsTableNode(ArgsTableNode node, TState state);

        TReturn VisitKeysTableNode(KeysTableNode node, TState state);

        TReturn VisitAssignNode(AssignNode node, TState state);

        TReturn VisitBinaryExpressionNode(BinaryExpressionNode node, TState state);

        TReturn VisitBlockNode(BlockNode node, TState state);

        TReturn VisitBreakNode(BreakNode node, TState state);

        TReturn VisitCallRedisMethodNode(CallRedisMethodNode node, TState state);

        TReturn VisitCastNode(CastNode node, TState state);

        TReturn VisitConditionalExpressionNode(ConditionalExpressionNode node, TState state);

        TReturn VisitConstantValueNode(ConstantValueNode node, TState state);

        TReturn VisitDoWhileNode(DoWhileNode node, TState state);

        TReturn VisitEmptyNode(EmptyNode node, TState state);

        TReturn VisitIdentifierNode(IdentifierNode node, TState state);

        TReturn VisitIfNode(IfNode node, TState state);

        TReturn VisitNilNode(NilNode node, TState state);

        TReturn VisitReturnNode(ReturnNode node, TState state);

        TReturn VisitStatusNode(StatusNode node, TState state);

        TReturn VisitTableAccessNode(TableKeyAccessNode node, TState state);

        TReturn VisitUnaryExpressionNode(UnaryExpressionNode node, TState state);

        TReturn VisitUniformOperatorNode(UniformOperatorNode node, TState state);

        TReturn VisitVariableDeclareNode(VariableDeclareNode node, TState state);

        TReturn VisitWhileNode(WhileNode node, TState state);

        TReturn VisitCallLuaMethodNode(CallLuaMethodNode node, TState state);

        TReturn VisitCursorNode(CursorNode node, TState state);

        TReturn VisitArrayTableDefinitionNode(ArrayTableDefinitionNode node, TState state);
    }
}