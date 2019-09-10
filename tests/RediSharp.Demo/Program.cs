using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RediSharp.Lib;
using RediSharp.Lua;
using StackExchange.Redis;

namespace RediSharp.Demo
{
    class Program
    {
        static RedisValue RedisFunction(IDatabase cursor, RedisValue[] args, RedisKey[] keys)
        {
            cursor.StringSet("name", "areller");
            return cursor.StringGet("name");
        }
        
        static async Task Main(string[] args)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync("localhost");

            Client<IDatabase> client = new SEClient(connection.GetDatabase(0));
            var handle = client.GetHandle(RedisFunction);

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