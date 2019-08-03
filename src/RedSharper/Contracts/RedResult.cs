using RedSharper.Contracts.Enums;

namespace RedSharper.Contracts
{
    public abstract class RedResult
    {
        public abstract RedResultType Type { get; }

        public abstract bool IsNull { get; }
    }
}