using System;
using System.Diagnostics;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp.IntegrationTests.Extensions
{
    public static class ClientExtensions
    {
        private static object _globalSync = new object();
        
        public static async Task<TRes> ExecuteP<TRes>(this Client<IDatabase> client, Function<IDatabase, TRes> action,
            RedisValue[] arguments = null, RedisKey[] keys = null)
        {
            var handle = client.GetHandle(action);
            await handle.Init();

            using (var writer = new System.IO.StreamWriter(System.Console.OpenStandardOutput()))
            {
                lock (_globalSync)
                {
                    Console.WriteLine("===========================");
                    Console.WriteLine(handle.Artifact);
                    Console.WriteLine("===========================");
                    
                    writer.WriteLine("===========================");
                    writer.WriteLine(handle.Artifact);
                    writer.WriteLine("===========================");
                }
            }

            var res = await handle.Execute(arguments, keys);
            return res;
        }
    }
}
