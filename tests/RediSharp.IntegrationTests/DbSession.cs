using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp.IntegrationTests
{
    /// <summary>
    /// Test Connection Pool that Supports Parallel Executing Tests
    /// </summary>
    public class DbSession : IDisposable
    {
        private int _dbNum;

        public IDatabase Db { get; }

        public Client<IDatabase> Client { get; }
        
        private DbSession(int dbNum, IDatabase db)
        {
            _dbNum = dbNum;
            Db = db;
            Client = new SEClient(db);
        }
        
        public void Dispose()
        {
            ReturnDbNum(_dbNum);
        }
        
        #region Static

        private static ConnectionMultiplexer _connection;

        private static SemaphoreSlim _connectionSyncObj;

        private static SemaphoreSlim _executionSyncObj;

        private static ConcurrentBag<int> _dbNumbers;

        static DbSession()
        {
            _connectionSyncObj = new SemaphoreSlim(1, 1);
            _executionSyncObj = new SemaphoreSlim(16, 16);
            _dbNumbers = new ConcurrentBag<int>(Enumerable.Range(0, 16));
        }

        private static async Task<int> CreateDbNum()
        {
            await _executionSyncObj.WaitAsync();
            int dbNum;
            if (!_dbNumbers.TryTake(out dbNum))
            {
                throw new Exception("Db Numbers bag was empty");
            }

            return dbNum;
        }
        
        private static void ReturnDbNum(int dbNum)
        {
            _dbNumbers.Add(dbNum);
            _executionSyncObj.Release();
        }

        private static async Task MakeSureConnected()
        {
            if (!(_connection is null)) return;

            await _connectionSyncObj.WaitAsync();
            try
            {
                if (_connection is null)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync("localhost,allowAdmin=true");
                }
            }
            finally
            {
                _connectionSyncObj.Release();
            }
        }
        
        public static async Task<DbSession> Create()
        {
            await MakeSureConnected();
            var dbNum = await CreateDbNum();

            try
            {
                await _connection.GetServer(_connection.GetEndPoints().First()).FlushDatabaseAsync(dbNum);
                return new DbSession(dbNum, _connection.GetDatabase(dbNum));
            }
            catch (Exception)
            {
                ReturnDbNum(dbNum);
                throw;
            }
        }
        
        #endregion
    }
}