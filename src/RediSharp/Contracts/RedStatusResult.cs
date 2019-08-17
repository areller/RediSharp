using System;
using RediSharp.Contracts.Enums;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp.Contracts
{
    public class RedStatusResult : RedResult
    {
        private readonly string value;

        internal override RedResultType Type { get; }

        public RedStatusResult(bool isError, string value)
        {
            this.value = value;
            this.Type = isError ? RedResultType.Error : RedResultType.SimpleString;
        }

        internal override bool IsNull => value == null;
        public override string ToString() => value;

        [RedILResolve(typeof(StatusIsOkResolver))]
        public bool IsOk => !IsNull && Type == RedResultType.SimpleString;

        [RedILResolve(typeof(StatusMessageResolver))]
        public string Message => value;
    }
}