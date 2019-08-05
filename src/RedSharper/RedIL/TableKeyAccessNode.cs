using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class TableKeyAccessNode : RedILNode
    {
        public RedILNode Table { get; set; }

        public RedILNode Key { get; set; }

        public TableKeyAccessNode() : base(RedILNodeType.TableKeyAccess) { }

        public TableKeyAccessNode(
            RedILNode table,
            RedILNode key
        ) : base(RedILNodeType.TableKeyAccess)
        { }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}