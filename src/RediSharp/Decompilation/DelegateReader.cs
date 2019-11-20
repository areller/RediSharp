using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata;
using System.Threading;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Metadata;

namespace RediSharp.Decompilation
{
    public static class DelegateReader
    {
        private static readonly IAssemblyProvider _provider = new RecursiveAssemblyProvider(true);
        private static readonly ConcurrentDictionary<Delegate, Lazy<SyntaxTree>> _cache = new ConcurrentDictionary<Delegate, Lazy<SyntaxTree>>();

        public static SyntaxTree ReadCached(Delegate @delegate)
        {
            Contract.Assert(@delegate != null);

            try
            {
                return _cache.GetOrAdd(@delegate, key => new Lazy<SyntaxTree>(() => Read(key), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
            }
            catch (Exception)
            {
                _cache.TryRemove(@delegate, out _);
                throw;
            }
        }
        
        public static SyntaxTree Read(Delegate @delegate)
        {
            Contract.Assert(@delegate?.Method?.DeclaringType != null);

            var asm = @delegate.Method.DeclaringType.Assembly;
            _provider.Prepare(asm);
            var decompiler = new CSharpDecompiler(asm.Location, _provider, new DecompilerSettings()
            {
                ExtensionMethods = false,
                NamedArguments = false,
                LiftNullables = true
            });
            var token = @delegate.Method.MetadataToken;
            var method = MetadataTokenHelpers.TryAsEntityHandle(token);
            var ast = decompiler.Decompile(new List<EntityHandle>()
            {
                method.Value
            });

            return ast;
        }
    }
}