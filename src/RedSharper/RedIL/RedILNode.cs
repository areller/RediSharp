using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    abstract class RedILNode
    {
        public RedILNodeType Type { get; }

        protected RedILNode(RedILNodeType type)
        {
            Type = type;
        }
    }
}