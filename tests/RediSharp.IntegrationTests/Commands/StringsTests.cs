using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.Contracts;
using RediSharp.IntegrationTests.Extensions;
using StackExchange.Redis;

namespace RediSharp.IntegrationTests.Commands
{
    [TestClass]
    public class StringsTests
    {
        [TestMethod]
        public async Task ShouldRunGet()
        {
            using (var sess = await DbSession.Create())
            {
                await sess.Db.StringSetAsync("hello", "world");
                var res = await sess.Client.ExecuteP<RedSingleResult>((cursor, args, keys) => cursor.Get("hello"));
                res.ToString().Should().Be("world");
            }
        }

        [TestMethod]
        public async Task ShouldRunSet()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<RedStatusResult>((cursor, args, keys) =>
                    cursor.Set("hello", "world"));
                res.IsOk.Should().BeTrue();
                (await sess.Db.StringGetAsync("hello")).Should().Be("world");
            }
        }
    }
}