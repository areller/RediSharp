using System;
using RedSharper.Contracts.Enums;
using StackExchange.Redis;

namespace RedSharper.Contracts
{ 
    public class RedArrayResult : RedResult
    {
        public override bool IsNull => _value == null;
        private readonly RedResult[] _value;

        public override RedResultType Type => RedResultType.MultiBulk;
        public RedArrayResult(RedResult[] value)
        {
            _value = value;
        }

        public override string ToString() => _value == null ? "(nil)" : (_value.Length + " element(s)");
    }
}