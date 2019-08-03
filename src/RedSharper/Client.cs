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
        private DecompilationStore _decompiler;

        public Client(IConnectionMultiplexer connection)
        {
            _decompiler = new DecompilationStore();
        }

        public async Task<RedResult> Execute(Func<ICursor, string[], RedResult> action, string[] keys = null)
        {

        }

        public async Task<RedResult> Execute<T>(Func<ICursor, string[], T, RedResult> action, T args, string[] keys = null)
            where T : struct, ITuple
        {
            return null;
        }
    }
}