using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.CSharp;
using RediSharp.RedIL;
using RediSharp.RedIL.Nodes;

namespace RediSharp.UnitTests.Resolving
{
    [TestClass]
    public class ExternalResolvingTests
    {
        private static ActionDecompiler _actionDecompiler;

        private static CSharpCompiler _csharpCompiler;

        [ClassInitialize]
        public static void ClassSetup(TestContext ctx)
        {
            _actionDecompiler = new ActionDecompiler(Assembly.GetCallingAssembly());
            _csharpCompiler = new CSharpCompiler();
        }

        [TestMethod]
        public void ShouldResolveMethodOfExternalInterface()
        {
            var csharp = _actionDecompiler.Decompile<ICursor, string>((cursor, args, keys) =>
            {
                ISomeInterface foo = new SomeInterace("abc");
                return foo.Greeting("def");
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }

        [TestMethod]
        public void ShouldResolveMethodOfExternalClass()
        {
            var csharp = _actionDecompiler.Decompile<ICursor, string>((cursor, args, keys) =>
            {
                var foo = new SomeInterace("abc");
                return foo.Greeting("def");
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }

        [TestMethod]
        public void ShouldResolveStaticMethodOfExternalClass()
        {
            var csharp = _actionDecompiler.Decompile<ICursor, string>((cursor, args, keys) =>
                {
                    return SomeInterace.StaticGreeting("def");
                });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }
        
        [TestMethod]
        public void ShouldResolveStaticMemberOfExternalClass()
        {
            var csharp = _actionDecompiler.Decompile<ICursor, string>((cursor, args, keys) =>
                {
                    return SomeInterace.SomeKey;
                });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }
    }
}