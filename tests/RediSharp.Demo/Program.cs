using RediSharp.Debugging;
using StackExchange.Redis;
using System;

namespace RediSharp.Demo
{
    //[DebugProcedureGenerator]
    [RedisProcedure]
    partial class MyProcedure : IRedisProcedure<string>
    {
        public string Define(IDatabase client, RedisValue[] args, RedisKey[] keys)
        {
            var c = 3 + 1;
            for (int i = 0; i < c + 1; i++)
            {
                client.StringSet($"name_{i}", "areller");
                var x = client.StringGet($"name_{i}");
            }

            return client.StringGet($"name_{c}");
        }
    }

    [RedisProcedure]
    public partial class MyProcedure2 : IRedisProcedure<string>
    {
        public string Define(IDatabase client, RedisValue[] args, RedisKey[] keys)
        {
            return client.StringGet(keys[0]);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(MyProcedure.GetLua());
            Console.WriteLine(MyProcedure2.GetLua());
        }
    }
}
