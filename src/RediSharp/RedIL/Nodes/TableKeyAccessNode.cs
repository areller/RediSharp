using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
{
    class TableKeyAccessNode : ExpressionNode
    {
        public ExpressionNode Table { get; set; }

        public ExpressionNode Key { get; set; }

        public TableKeyAccessNode() : base(RedILNodeType.TableKeyAccess) { }

        public TableKeyAccessNode(
            ExpressionNode table,
            ExpressionNode key,
            DataValueType type)
            : base(RedILNodeType.TableKeyAccess, type)
        {
            Table = table;
            Key = key;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitTableAccessNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is TableKeyAccessNode)) return false;
            var tableAccess = (TableKeyAccessNode) other;
            return Table.EqualOrNull(tableAccess.Table) &&
                   Key.EqualOrNull(tableAccess.Key);
        }

        public override ExpressionNode Simplify() => new TableKeyAccessNode(Table.Simplify(), Key.Simplify(), DataType);
    }
}