using System;
using System.Threading.Tasks;
using RediSharp.Contracts;
using StackExchange.Redis;

namespace RediSharp.IntegrationTests.Extensions
{
    public static class ClientExtensions
    {
        public static async Task<TRes> ExecuteP<TRes>(this Client client, Func<ICursor, RedisValue[], RedisKey[], TRes> action,
            RedisValue[] arguments = null, RedisKey[] keys = null)
            where TRes : RedResult
        {
            var handle = client.GetLuaHandle(action);
            await handle.Init();
            
            Console.WriteLine(handle.Artifact);

            var res = await handle.Execute(arguments, keys);
            return res;
        }
    }
}