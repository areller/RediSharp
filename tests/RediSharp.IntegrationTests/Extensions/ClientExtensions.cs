using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using RediSharp.Decompilation;
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

            using (var writer = new StreamWriter(System.Console.OpenStandardOutput()))
            {
                lock (_globalSync)
                {
                    Console.WriteLine("=========================== START");
                    Console.WriteLine(DelegateReader.Read(action));
                    Console.WriteLine("===========================");
                    Console.WriteLine(handle.Artifact);
                    Console.WriteLine("=========================== END");
                    
                    writer.WriteLine("=========================== START");
                    writer.WriteLine(DelegateReader.Read(action));
                    writer.WriteLine("===========================");
                    writer.WriteLine(handle.Artifact);
                    writer.WriteLine("=========================== END");
                }
            }

            var res = await handle.Execute(arguments, keys);
            return res;
        }
    }
}