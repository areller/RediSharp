using System;
using RedSharper.Contracts.Enums;
using StackExchange.Redis;

namespace RedSharper.Contracts
{
    public class RedErrorResult : RedResult
    {
        private readonly string value;

        public override RedResultType Type => RedResultType.Error;
        
        public RedErrorResult(string value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override bool IsNull => value == null;
        public override string ToString() => value;
    }
}