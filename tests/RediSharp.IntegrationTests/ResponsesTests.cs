using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.IntegrationTests.Extensions;
using StackExchange.Redis;

namespace RediSharp.IntegrationTests
{
    [TestClass]
    public class ResponsesTests
    {
        //TODO: Explore code generation for these tests
        
        /*
         * These tests check that the following types
         * int, int?, int[], IList<int>
         * long, long?, long[], IList<long>,
         * ulong, ulong?, ulong[], IList<ulong[]>
         * double, double?, double[], IList<long>,
         * decimal, decimal?, decimal[], IList<decimal>,
         * float, float?, float[], IList<float>,
         * bool, bool?, bool[], IList<bool>,
         * string, string[], IList<string>,
         * byte[], byte[][], IList<byte[]>,
         * RedisValue, RedisValue[], IList<RedisValue>,
         * RedisKey, RedisKey[], IList<RedisKey>
         * Convert from Lua to C#
         */
        
        #region int, int?, int[], IList<int>
        
        [TestMethod]
        public async Task ShouldReturnInt()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<int>((cursor, args, keys) => 420);
                res.Should().Be(420);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ShouldReturnOptInt(bool notNull)
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<int?>((cursor, args, keys) => (bool) args[0] ? (int?) 420 : null,
                    new[] {(RedisValue) notNull});
                res.Should().Be(notNull ? (int?) 420 : null);
            }
        }

        [TestMethod]
        public async Task ShouldReturnIntArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<int[]>((cursor, args, keys) => new int[] {1, 2, 3});
                res.Should().BeEquivalentTo(new int[] {1, 2, 3}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListInt()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<int>>((cursor, args, keys) => new List<int> {1, 2, 3});
                res.Should().BeEquivalentTo(new int[] {1, 2, 3}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region long, long?, long[], IList<long>
        
        [TestMethod]
        public async Task ShouldReturnLong()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<long>((cursor, args, keys) => 420);
                res.Should().Be(420);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ShouldReturnOptLong(bool notNull)
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<long?>((cursor, args, keys) => (bool) args[0] ? (long?) 420 : null,
                    new[] {(RedisValue) notNull});
                res.Should().Be(notNull ? (long?) 420 : null);
            }
        }

        [TestMethod]
        public async Task ShouldReturnLongArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<long[]>((cursor, args, keys) => new long[] {1, 2, 3});
                res.Should().BeEquivalentTo(new long[] {1, 2, 3}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListLong()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<long>>((cursor, args, keys) => new List<long> {1, 2, 3});
                res.Should().BeEquivalentTo(new long[] {1, 2, 3}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region ulong, ulong?, ulong[], IList<ulong>
        
        [TestMethod]
        public async Task ShouldReturnULong()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<ulong>((cursor, args, keys) => 420);
                res.Should().Be(420);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ShouldReturnOptULong(bool notNull)
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<ulong?>((cursor, args, keys) => (bool) args[0] ? (ulong?) 420 : null,
                    new[] {(RedisValue) notNull});
                res.Should().Be(notNull ? (ulong?) 420 : null);
            }
        }

        [TestMethod]
        public async Task ShouldReturnULongArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<ulong[]>((cursor, args, keys) => new ulong[] {1, 2, 3});
                res.Should().BeEquivalentTo(new ulong[] {1, 2, 3}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListULong()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<ulong>>((cursor, args, keys) => new List<ulong> {1, 2, 3});
                res.Should().BeEquivalentTo(new ulong[] {1, 2, 3}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region double, double?, double[], IList<double>
        
        /*
        [TestMethod]
        public async Task ShouldReturnDouble()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<double>((cursor, args, keys) => 420.69);
                res.Should().Be(420.69);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ShouldReturnOptDouble(bool notNull)
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<double?>((cursor, args, keys) => (bool) args[0] ? (double?) 420.69 : null,
                    new[] {(RedisValue) notNull});
                res.Should().Be(notNull ? (double?) 420.69 : null);
            }
        }

        [TestMethod]
        public async Task ShouldReturnDoubleArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<double[]>((cursor, args, keys) => new double[] {1.1, 2.2, 3});
                res.Should().BeEquivalentTo(new double[] {1.1, 2.2, 3}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListDouble()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<double>>((cursor, args, keys) => new List<double> {1.1, 2.2, 3});
                res.Should().BeEquivalentTo(new double[] {1.1, 2.2, 3}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region float, float?, float[], IList<float>
        
        [TestMethod]
        public async Task ShouldReturnFloat()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<float>((cursor, args, keys) => 420.69f);
                res.Should().Be(420.69f);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ShouldReturnOptFloat(bool notNull)
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<float?>((cursor, args, keys) => (bool) args[0] ? (float?) 420.69f : null,
                    new[] {(RedisValue) notNull});
                res.Should().Be(notNull ? (float?) 420.69f : null);
            }
        }

        [TestMethod]
        public async Task ShouldReturnFloatArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<float[]>((cursor, args, keys) => new float[] {1.1f, 2.2f, 3});
                res.Should().BeEquivalentTo(new float[] {1.1f, 2.2f, 3}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListFloat()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<float>>((cursor, args, keys) => new List<float> {1.1f, 2.2f, 3});
                res.Should().BeEquivalentTo(new float[] {1.1f, 2.2f, 3}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region decimal, decimal?, decimal[], IList<decimal>
        
        [TestMethod]
        public async Task ShouldReturnDecimal()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<decimal>((cursor, args, keys) => 420.69M);
                res.Should().Be(420.69M);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ShouldReturnOptDecimal(bool notNull)
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<decimal?>((cursor, args, keys) => (bool) args[0] ? (decimal?) 420.69M : null,
                    new[] {(RedisValue) notNull});
                res.Should().Be(notNull ? (decimal?) 420.69M : null);
            }
        }

        [TestMethod]
        public async Task ShouldReturnDecimalArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<decimal[]>((cursor, args, keys) => new decimal[] {1.1M, 2.2M, 3});
                res.Should().BeEquivalentTo(new decimal[] {1.1M, 2.2M, 3}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListDecimal()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<decimal>>((cursor, args, keys) => new List<decimal> {1.1M, 2.2M, 3});
                res.Should().BeEquivalentTo(new decimal[] {1.1M, 2.2M, 3}, opts => opts.WithStrictOrdering());
            }
        }
        */
        
        #endregion
        
        #region bool, bool?, bool[], IList<bool>

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ShouldReturnBool(bool value)
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<bool>((cursor, args, keys) => (bool) args[0],
                    new[] {(RedisValue) value});
                res.Should().Be(value);
            }
        }

        [TestMethod]
        [DataRow(false, true)]
        [DataRow(false, false)]
        [DataRow(true, false)]
        public async Task ShouldReturnOptBool(bool isNull, bool value)
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<bool?>(
                    (cursor, args, keys) => (bool) args[0] ? (bool?) null : (bool) args[1],
                    new[] {(RedisValue) isNull, (RedisValue) value});
                res.Should().Be(value);
            }
        }

        [TestMethod]
        public async Task ShouldReturnBoolArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<bool[]>((cursor, args, keys) => new bool[] {true, false, true});
                res.Should().BeEquivalentTo(new bool[] {true, false, true}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListBool()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<bool>>((cursor, args, keys) =>
                    new List<bool>() {true, false, true});
                res.Should().BeEquivalentTo(new bool[] {true, false, true}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region string, string[], IList<string>

        [TestMethod]
        public async Task ShouldReturnString()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<string>((cursor, args, keys) => "Hello");
                res.Should().Be("Hello");
            }
        }

        [TestMethod]
        public async Task ShouldReturnStringArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<string[]>((cursor, args, keys) => new string[] {"Hello", "World"});
                res.Should().BeEquivalentTo(new string[] {"Hello", "World"}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListString()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<string>>((cursor, args, keys) =>
                    new List<string>() {"Hello", "World"});
                res.Should().BeEquivalentTo(new string[] {"Hello", "World"}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region byte[], byte[][], IList<byte[]>

        public async Task ShouldReturnByteArray()
        {
            using (var sess = await DbSession.Create())
            {
                
            }
        }
        
        #endregion
        
        #region RedisValue, RedisValue[], IList<RedisValue>

        [TestMethod]
        [DataRow("Hello")]
        [DataRow(5)]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ShouldReturnRedisValue(object value)
        {
            var redisValue = GetFromObject(value);
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<RedisValue>((cursor, args, keys) => args[0], new[] {redisValue});
                res.Should().Be(redisValue);
            }
        }

        [TestMethod]
        public async Task ShouldReturnRedisValueArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<RedisValue[]>((cursor, args, keys) =>
                    new RedisValue[] {"Hello", 1, true});
                res.Should().BeEquivalentTo(new RedisValue[] {"Hello", 1, true}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListRedisValue()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<RedisValue>>((cursor, args, keys) =>
                    new List<RedisValue>() {"Hello", 1, true});
                res.Should().BeEquivalentTo(new RedisValue[] {"Hello", 1, true}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region RedisKey, RedisKey[], IList<RedisKey>

        [TestMethod]
        public async Task ShouldReturnRedisKey()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<RedisKey>((cursor, args, keys) => (RedisKey) "key");
                res.Should().Be("key");
            }
        }

        [TestMethod]
        public async Task ShouldReturnRedisKeyArray()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<RedisKey[]>(
                    (cursor, args, keys) => new RedisKey[] {"key1", "key2"});
                res.Should().BeEquivalentTo(new RedisKey[] {"key1", "key2"}, opts => opts.WithStrictOrdering());
            }
        }

        [TestMethod]
        public async Task ShouldReturnIListRedisKey()
        {
            using (var sess = await DbSession.Create())
            {
                var res = await sess.Client.ExecuteP<IList<RedisKey>>(
                    (cursor, args, keys) => new List<RedisKey> {"key1", "key2"});
                res.Should().BeEquivalentTo(new RedisKey[] {"key1", "key2"}, opts => opts.WithStrictOrdering());
            }
        }
        
        #endregion
        
        #region Helpers
        
        private RedisValue GetFromObject(object value)
        {
            switch (value)
            {
                case int intVal: return (RedisValue) intVal;
                case long longVal: return (RedisValue) longVal;
                case ulong ulongVal: return (RedisValue) ulongVal;
                case bool boolVal: return (RedisValue) boolVal;
                case string stringVal: return (RedisValue) stringVal;
                case double doubleVal: return (RedisValue) doubleVal;
                case byte[] byteVal: return (RedisValue) byteVal;
                default: throw new Exception("Invalid type");
            }
        }
        
        #endregion
    }
}