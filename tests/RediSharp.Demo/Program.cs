using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp.Demo
{
    class Program
    {
        static bool RedisFunction(ICursor cursor, RedisValue[] args, RedisKey[] keys)
        {
            var arr = new int[] {1, 2, 3};
            return cursor.Set("key", arr.Length);
        }
        
        static async Task Main(string[] args)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync("localhost");

            Client<ICursor> client = new Client<ICursor>(connection.GetDatabase(0));
            var handle = client.GetLuaHandle(RedisFunction);

            // Printing Lua
            Console.WriteLine("===========================");
            Console.WriteLine(handle.Artifact);
            Console.WriteLine("===========================");

            await handle.Init();
            var res = await handle.Execute(new RedisValue[] {5}, new RedisKey[] {"countKey"});
            
            Console.WriteLine(res);
        }
    }
}