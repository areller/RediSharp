using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class BinaryExpressionNode : RedILNode
    {
        public BinaryExpressionOperator Operator { get; }

        public RedILNode Left { get; }

        public RedILNode Right { get; }

        public BinaryExpressionNode(
            BinaryExpressionOperator op,
            RedILNode left,
            RedILNode right)
            : base(RedILNodeType.BinaryExpression)
        {
            Operator = op;
            Left = left;
            Right = right;
        }
    }
}