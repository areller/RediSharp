using System;
using RedSharper.Contracts.Enums;
using RedSharper.RedIL.Attributes;
using StackExchange.Redis;

namespace RedSharper.Contracts
{
    public abstract class RedResult
    {
        internal abstract RedResultType Type { get; }

        internal abstract bool IsNull { get; }

        [RedILResolve(typeof(OkStatusResolver))]
        public static RedStatusResult Ok => new RedStatusResult(false, "OK");
        
        [RedILResolve(typeof(ErrorStatusResolver))]
        public static RedStatusResult Error(string message = null) => new RedStatusResult(true, message);
    }
}