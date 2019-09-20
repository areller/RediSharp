using System.Reflection;
using LiveDelegate.ILSpy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.RedIL;
using RediSharp.RedIL.Nodes;

namespace RediSharp.UnitTests.Resolving
{
    [TestClass]
    public class ArrayResolvingTests
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
        public void ShouldResolveArrayLength()
        {
            var csharp = DecompilationResult.CreateFromDelegate<NullCursor, int>(_delegateReader, (cursor, args, keys) =>
            {
                var arr = new int[] {1, 2, 3};
                return arr.Length;
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }
    }
}