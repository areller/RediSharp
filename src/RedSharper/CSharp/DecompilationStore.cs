using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Metadata;
using RedSharper.Contracts;

namespace RedSharper.CSharp
{
    class DecompilationStore
    {
        private Assembly _rootAssembly;

        private AssemblyResolver _assemblyResolver;

        private CSharpDecompiler _decompiler;

        private ConcurrentDictionary<object, Lazy<DecompilationResult>> _cache;

        public DecompilationStore()
        {
            _rootAssembly = Assembly.GetEntryAssembly();
            _cache = new ConcurrentDictionary<object, Lazy<DecompilationResult>>();
            _assemblyResolver = new AssemblyResolver(_rootAssembly);

            var file = _rootAssembly.Location;
            var decompiler = new CSharpDecompiler(file, _assemblyResolver, new DecompilerSettings());

            _decompiler = decompiler;
        }

        public DecompilationResult Decompile<T>(Func<ICursor, string[], T, RedResult> action)
        {
            var token = action.Method.MetadataToken;
            var method = MetadataTokenHelpers.TryAsEntityHandle(token);

            var ast = _decompiler.Decompile(new List<EntityHandle>()
            {
                method.Value
            });

            return ExtractTreeAndMetadata(ast);
        }

        public DecompilationResult Decompile(Func<ICursor, string[], RedResult> action)
        {
            var token = action.Method.MetadataToken;
            var method = MetadataTokenHelpers.TryAsEntityHandle(token);

            var ast = _decompiler.Decompile(new List<EntityHandle>()
            {
                method.Value
            });

            return ExtractTreeAndMetadata(ast);
        }

        private DecompilationResult ExtractTreeAndMetadata(SyntaxTree tree)
        {
            
        }
    }
}