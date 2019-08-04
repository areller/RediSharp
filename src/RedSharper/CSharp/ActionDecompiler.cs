using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Metadata;
using RedSharper.Contracts;

namespace RedSharper.CSharp
{
    class ActionDecompiler
    {
        private Assembly _rootAssembly;

        private AssemblyResolver _assemblyResolver;

        private CSharpDecompiler _decompiler;

        public ActionDecompiler()
        {
            _rootAssembly = Assembly.GetEntryAssembly();
            _assemblyResolver = new AssemblyResolver(_rootAssembly);

            var file = _rootAssembly.Location;
            var decompiler = new CSharpDecompiler(file, _assemblyResolver, new DecompilerSettings());

            _decompiler = decompiler;
        }

        public DecompilationResult Decompile<T>(Func<ICursor, string[], T, RedResult> action)
            where T : struct
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
            var firstMethodDeclaration = tree.Children.First(c => c.GetType().Name == typeof(MethodDeclaration).Name) as MethodDeclaration;
            return null;
        }
    }
}