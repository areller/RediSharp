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
using StackExchange.Redis;

namespace RediSharp.CSharp
{
    class ActionDecompiler
    {
        private Assembly _rootAssembly;

        private Assembly _callingAssembly;

        private AssemblyResolver _assemblyResolver;

        private CSharpDecompiler _decompiler;

        public ActionDecompiler(Assembly callingAssembly)
        {
            _callingAssembly = callingAssembly;
            _rootAssembly = Assembly.GetEntryAssembly();
            _assemblyResolver = new AssemblyResolver(_rootAssembly, _callingAssembly);

            var file = _callingAssembly.Location;
            var decompiler = new CSharpDecompiler(file, _assemblyResolver, new DecompilerSettings()
            {
                ExtensionMethods = false
            });

            _decompiler = decompiler;
        }

        public DecompilationResult Decompile<TCursor, TRes>(Func<TCursor, RedisValue[], RedisKey[], TRes> action)
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
            /*
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

            return new DecompilationResult(_rootAssembly, firstMethodDeclaration.Body, cursorName, argsName, keysName, argsSubKeys);
            */
            
            var firstMethodDeclaration = tree.Children.First(c => c.GetType().Name == typeof(MethodDeclaration).Name) as MethodDeclaration;
            var methodParameters = firstMethodDeclaration.Parameters.ToArray();

            string cursorName = methodParameters[0].Name;
            string argsName = methodParameters[1].Name;
            string keysName = methodParameters[2].Name;

            return new DecompilationResult(_rootAssembly, firstMethodDeclaration.Body, cursorName, argsName, keysName,
                null);
        }
    }
}