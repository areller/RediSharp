using System;
using StackExchange.Redis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RedSharper.CSharp;
using RedSharper.Contracts;

namespace RedSharper
{
    public class Client
    {
        private ActionDecompiler _decompiler;

        public Client(IConnectionMultiplexer connection)
        {
            _decompiler = new ActionDecompiler();
        }

        public async Task<RedResult> Execute(Func<ICursor, string[], RedResult> action, string[] keys = null)
        {
            return null;
        }

        public async Task<RedResult> Execute<T>(Func<ICursor, string[], T, RedResult> action, T args, string[] keys = null)
            where T : struct
        {
            return null;
        }
    }
}