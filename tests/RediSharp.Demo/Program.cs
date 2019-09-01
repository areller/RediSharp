using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RediSharp.Lib;
using StackExchange.Redis;

namespace RediSharp.Demo
{
    class Program
    {
        static RedisValue[] RedisFunction(IDatabase cursor, RedisValue[] args, RedisKey[] keys)
        {
            var dict = new Dictionary<string, List<int>>()
            {
                {"abc", new List<int>() {1, 2, 3}},
                {"cde", new List<int>() {3, 4, 5}}
            };

            foreach (var elem in dict)
            {
                cursor.SetAdd("names", elem.Key);
                foreach (var num in elem.Value)
                {
                    cursor.SetAdd($"{elem.Key}_nums", num);
                }
            }

            var union = cursor.SetCombine(SetOperation.Union, new RedisKey[] {"abc_nums", "cde_nums"});
            var ts = TimeSpan.FromSeconds((int?) cursor.StringGet("exp") ?? 5);
            cursor.StringSet("json", Json.Encode(union), ts);

            return union;
        }
        
        static async Task Main(string[] args)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync("localhost");

            Client<IDatabase> client = new Client<IDatabase>(connection.GetDatabase(0));
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