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
        static async Task Main(string[] args)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync("localhost");

            Client client = new Client(connection.GetDatabase(0));
            var res = await client.Execute((cursor, argv, keys) =>
            {
                var count = cursor.Get(keys[0]).AsInt();
                var toAdd = (int) argv[0];

                for (var i = 0; i < count; i++)
                {
                    var key = keys[0] + i;
                    var currentValue = cursor.Get(key).AsLong() ?? 0;
                    cursor.Set(key, currentValue + toAdd);
                }

                return RedResult.Ok;
            }, new RedisValue[] {5}, new RedisKey[] {"countKey"});
            
            Console.WriteLine(res);
        }
    }
}