using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp.Demo
{
    class Program
    {
        static bool RedisFunction(IDatabase cursor, RedisValue[] args, RedisKey[] keys)
        {
            var ts = TimeSpan.FromSeconds(100);
            return cursor.StringSet("Hello", "World", ts);;
        }
        
        static bool RedisFunction2(ICursor cursor, RedisValue[] args, RedisKey[] keys)
        {
            /*
            var arr = new int[] {1, 2, 3};
            string text = string.Concat("A", "B");
            string[] parts = text.Split(",");
            List<int> lst2 = new List<int>();
            lst2.Add(3);
            IList<int> lst = new List<int>();
            lst.Add(2);
            cursor.Set("key2", lst.Count);
            return cursor.Set("key", arr.GetLength(0));*/

            var dd = new Dictionary<string, int>()
            {
                {"Arik", 23},
                {"John", 21}
            };
            var kv = new KeyValuePair<int, int>(1, 2);
            var ts = new TimeSpan(1, 1, 1);
            
            var t1 = TimeSpan.FromDays(1);
            var t2 = TimeSpan.FromDays(2);
            var arr = new int[] {1, 2};
            var list = new List<int>() {3, 4};
            var arr2 = new RedisValue[] {"a", "b"};
            
            var dict = new Dictionary<int, int>();
            dict.Add(1, 2);
            var list3 = new List<int>();
            list3.Add(4);

            cursor.Set("length", arr.Length);
            cursor.Set("length2", list.Count);
            cursor.Set("length3", arr2.Length);
            cursor.Set("length4", arr.GetLength(0));
            cursor.Set("length5", list.IndexOf(3));
            cursor.Set("key", (t1 + t2).Seconds);

            return true;
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