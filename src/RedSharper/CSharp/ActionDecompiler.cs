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
using StackExchange.Redis;

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
            var decompiler = new CSharpDecompiler(file, _assemblyResolver, new DecompilerSettings()
            {
                ExtensionMethods = false
            });

            _decompiler = decompiler;
        }

        public DecompilationResult Decompile<TArgs, TRes>(Func<Cursor, RedisKey[], TArgs, TRes> action)
            where TArgs : struct
            where TRes : RedResult
        {
            var token = action.Method.MetadataToken;
            var method = MetadataTokenHelpers.TryAsEntityHandle(token);

            var ast = _decompiler.Decompile(new List<EntityHandle>()
            {
                method.Value
            });

            return ExtractTreeAndMetadata(ast);
        }

        public DecompilationResult Decompile<TRes>(Func<Cursor, RedisKey[], TRes> action)
            where TRes : RedResult
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
            var methodParameters = firstMethodDeclaration.Parameters.ToArray();

            string cursorName = methodParameters[0].Name;
            string keysName = methodParameters[1].Name;
            string argsName = null;
            string[] argsSubKeys = null;

            if (methodParameters.Length == 3)
            {
                argsName = methodParameters[2].Name;
                if (methodParameters[2].Type is TupleAstType)
                {
                    var tupleType = methodParameters[2].Type as TupleAstType;
                    argsSubKeys = tupleType.Children.Select(child => (child as TupleTypeElement).Name).ToArray();
                }
                else
                {
                    argsSubKeys = null;
                }
            }

            return new DecompilationResult(firstMethodDeclaration.Body, cursorName, argsName, keysName, argsSubKeys);
        }
    }
}