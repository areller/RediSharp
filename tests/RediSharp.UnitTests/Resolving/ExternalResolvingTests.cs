using System.Reflection;
using LiveDelegate.ILSpy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.RedIL;
using RediSharp.RedIL.Nodes;

namespace RediSharp.UnitTests.Resolving
{
    [TestClass]
    public class ExternalResolvingTests
    {
        private static IDelegateReader _delegateReader;

        private static CSharpCompiler _csharpCompiler;

        [ClassInitialize]
        public static void ClassSetup(TestContext ctx)
        {
            _delegateReader = DelegateReader.CreateWithDefaultAssemblyProvider();
            _csharpCompiler = new CSharpCompiler();
        }

        [TestMethod]
        public void ShouldResolveMethodOfExternalInterface()
        {
            var csharp = DecompilationResult.CreateFromDelegate<NullCursor, string>(_delegateReader, (cursor, args, keys) =>
            {
                ISomeInterface foo = new SomeInterace("abc");
                return foo.Greeting("def");
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }

        [TestMethod]
        public void ShouldResolveMethodOfExternalClass()
        {
            var csharp = DecompilationResult.CreateFromDelegate<NullCursor, string>(_delegateReader, (cursor, args, keys) =>
            {
                var foo = new SomeInterace("abc");
                return foo.Greeting("def");
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }

        [TestMethod]
        public void ShouldResolveStaticMethodOfExternalClass()
        {
            var csharp = DecompilationResult.CreateFromDelegate<NullCursor, string>(_delegateReader, (cursor, args, keys) =>
                {
                    return SomeInterace.StaticGreeting("def");
                });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }
        
        [TestMethod]
        public void ShouldResolveStaticMemberOfExternalClass()
        {
            var csharp = DecompilationResult.CreateFromDelegate<NullCursor, string>(_delegateReader, (cursor, args, keys) =>
                {
                    return SomeInterace.SomeKey;
                });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }
    }
}