using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp.IntegrationTests.Extensions
{
    public static class ClientExtensions
    {
        public static async Task<TRes> ExecuteP<TRes>(this Client<IDatabase> client, Function<IDatabase, TRes> action,
            RedisValue[] arguments = null, RedisKey[] keys = null)
        {
            var handle = client.GetHandle(action);
            await handle.Init();
            
            Console.WriteLine(handle.Artifact);

            var res = await handle.Execute(arguments, keys);
            return res;
        }
    }
}