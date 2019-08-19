using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.IntegrationTests.Extensions;
using StackExchange.Redis;

namespace RediSharp.IntegrationTests
{
    [TestClass]
    public class ForLoopsTests
    {
        #region Functions

        private bool FunctionA(ICursor cursor, RedisValue[] args, RedisKey[] keys)
        {
            var count = (int?) cursor.Get(keys[0]) ?? 0;
            var toAdd = (int) args[0];

            for (int i = 0; i < count; i++)
            {
                var key = keys[0] + "_" + i;
                var currentValue = (long?) cursor.Get(key) ?? 0;
                cursor.Set(key, currentValue + toAdd);
            }

            return true;
        }

        #endregion
        
        [TestMethod]
        public async Task ShouldRunFunctionA()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP(FunctionA, new RedisValue[] {5}, new RedisKey[] {"someKey"});
                res.Should().BeTrue();
                (await sess.Db.StringGetAsync("someKey_0")).Should().Be(5);
                res = await sess.Client.ExecuteP(FunctionA, new RedisValue[] {5}, new RedisKey[] {"someKey"});
                res.Should().BeTrue();
                (await sess.Db.StringGetAsync("someKey_0")).Should().Be(10);
            }
        }

        [TestMethod]
        [DataRow(3)]
        public async Task ShouldRunFunctionAWithExistingCount(int count)
        {
            using (var sess = await DbSession.Create())
            {
                await sess.Db.StringSetAsync("someKey", count);
                var res = await sess.Client.ExecuteP(FunctionA, new RedisValue[] {5}, new RedisKey[] {"someKey"});
                res.Should().BeTrue();
                for (int i = 0; i < count; i++)
                {
                    (await sess.Db.StringGetAsync("someKey_" + i)).Should().Be(5);
                }
                res = await sess.Client.ExecuteP(FunctionA, new RedisValue[] {5}, new RedisKey[] {"someKey"});
                res.Should().BeTrue();
                for (int i = 0; i < count; i++)
                {
                    (await sess.Db.StringGetAsync("someKey_" + i)).Should().Be(10);
                }
            }
        }
    }
}