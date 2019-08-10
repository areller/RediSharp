using System;
using RedSharper.Contracts.Enums;
using StackExchange.Redis;

namespace RedSharper.Contracts
{
    public abstract class RedResult
    {
        public abstract RedResultType Type { get; }

        public abstract bool IsNull { get; }

        public static RedResult Ok => new RedErrorResult(null);
    }
}