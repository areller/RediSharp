namespace RedSharper.RedIL
{
    interface IRedILVisitor<TState>
    {
        void VisitBlockNode(BlockNode block, TState state);

        void VisitVariableDeclareNode(VariableDeclareNode variableDeclare, TState state);

        void VisitAssignNode(AssignNode assign, TState state);

        void VisitIfNode(IfNode @if, TState state);
    }
}