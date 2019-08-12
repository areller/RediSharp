using System;
using System.Threading.Tasks;
using RedSharper.Contracts;
using RedSharper.Contracts.Extensions;
using RedSharper.Extensions;
using StackExchange.Redis;

namespace RedSharper.Demo
{
    class Program
    {
        
        static RedStatusResult RedisFunction(Cursor cursor, RedisValue[] args, RedisKey[] keys)
        {
            var count = cursor.Get(keys[0]).AsInt();
            var toAdd = (int) args[0];

            for (var i = 0; i < count; i++)
            {
                var key = $"{keys[0]}_{i}";
                var currentValue = cursor.Get(key).AsLong() ?? 0;
                var res = cursor.Set(key, currentValue + toAdd);
                if (!res.IsOk)
                {
                    // Reverting
                    for (var j = i - 1; j >= 0; j--)
                    {
                        key = keys[0] + "_" + j;
                        var newVal = cursor.Get(key).AsLong();
                        if (newVal != null)
                        {
                            cursor.Set(key, newVal - toAdd);
                        }
                    }

                    return RedResult.Error();
                }
            }

            return RedResult.Ok;
        }
        
        static async Task Main(string[] args)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync("localhost");

            Client client = new Client(connection.GetDatabase(0));
            var handle = client.GetHandleWithArtifact<RedStatusResult, string>(RedisFunction);

            // Printing Lua
            Console.WriteLine("===========================");
            Console.WriteLine(handle.Artifact);
            Console.WriteLine("===========================");

            await handle.Init();
            var res = await handle.Execute<RedStatusResult>(new RedisValue[] {5}, new RedisKey[] {"countKey"});
            Console.WriteLine(res.IsOk);
        }
    }
}