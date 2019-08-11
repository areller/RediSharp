using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class TableKeyAccessNode : ExpressionNode
    {
        public ExpressionNode Table { get; set; }

        public ExpressionNode Key { get; set; }

        public TableKeyAccessNode() : base(RedILNodeType.TableKeyAccess) { }

        public TableKeyAccessNode(
            ExpressionNode table,
            ExpressionNode key
        ) : base(RedILNodeType.TableKeyAccess)
        {
            Table = table;
            Key = key;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitTableAccessNode(this, state);
    }
}