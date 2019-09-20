using System;
using System.Diagnostics;
using System.Reflection;
using LiveDelegate.ILSpy;
using RediSharp.Lua;
using RediSharp.RedIL;
using StackExchange.Redis;

namespace RediSharp
{
    public class Client<TCursor> where TCursor : class
    {
        #region Private
        
        private CSharpCompiler _csharpCompiler;

        private LuaHandler _luaHandler;

        private IDelegateReader _delegateReader;

        #endregion
        
        /// <summary>
        /// A cursor implementation that is used during debugging
        /// </summary>
        public TCursor DebugInstance;
        
        /// <summary>
        /// A connection instance to a Redis database
        /// </summary>
        public IDatabase Database { get; }
        
        #region Options

        public bool DebuggingEnabled { get; set; } = true;

        #endregion
        
        public Client()
            : this(null, Assembly.GetCallingAssembly())
        {
        }

        public Client(IDatabase db)
            : this(db, Assembly.GetCallingAssembly())
        {
        }

        internal Client(IDatabase db, Assembly assembly)
            : this(db, assembly, default)
        {
        }

        internal Client(IDatabase db, TCursor debugInstance)
            : this(db, Assembly.GetCallingAssembly(), debugInstance)
        {
        }

        internal Client(IDatabase db, Assembly assembly, TCursor debugInstance)
        {
            _csharpCompiler = new CSharpCompiler();
            _luaHandler = new LuaHandler(db);
            _delegateReader = DelegateReader.CreateCachedWithDefaultAssemblyProvider();

            DebugInstance = debugInstance;
            Database = db;
        }

        /// <summary>
        /// Accepts a Function delegate and creates a non-initialized handle for executing the function
        /// </summary>
        /// <param name="function">A delegate with the function to execute</param>
        /// <typeparam name="TRes">The return type of the function</typeparam>
        /// <returns>A non-initialized handle for executing the function</returns>
        public IHandle<TRes> GetHandle<TRes>(Function<TCursor, TRes> function)
        {
            var decompilation = DecompilationResult.CreateFromDelegate(_delegateReader, function);
            var redIL = _csharpCompiler.Compile(decompilation);

            // We create the Lua handle regardless of whether we in Debug or not
            // Because we still want to fail/throw if RedIL/Lua compilation has failed
            var luaHandle = _luaHandler.CreateHandle<TRes>(redIL);

            if (DebuggingEnabled && Debugger.IsAttached && !(DebugInstance is null))
            {
                return new DebugHandle<TCursor, TRes>(luaHandle, DebugInstance, function);
            }

            return luaHandle;
        }
    }
}