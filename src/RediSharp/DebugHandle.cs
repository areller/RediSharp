using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp
{
    class DebugHandle<TCursor, TRes> : IHandle<TRes>
    {
        #region Private
        
        private IHandle<TRes> _realHandle;

        private TCursor _debugInstance;

        private Function<TCursor, TRes> _function;
        
        #endregion

        public object Artifact => _realHandle.Artifact;

        public bool IsInitialized => _realHandle.IsInitialized;

        public DebugHandle(IHandle<TRes> realHandle, TCursor debugInstance, Function<TCursor, TRes> function)
        {
            _realHandle = realHandle;
            _debugInstance = debugInstance;
            _function = function;
        }

        public Task Init() => _realHandle.Init();

        public Task<TRes> Execute(RedisValue[] args, RedisKey[] keys)
        {
            if (!IsInitialized)
            {
                throw new HandleException("Handle was not initialized");
            }

            return Task.FromResult(_function(_debugInstance, args, keys));
        }
    }
}