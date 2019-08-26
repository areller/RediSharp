using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.CSharp;
using RediSharp.RedIL;
using RediSharp.RedIL.Nodes;

namespace RediSharp.UnitTests.Resolving
{
    [TestClass]
    public class ArrayResolvingTests
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
        public void ShouldResolveArrayLength()
        {
            var csharp = _actionDecompiler.Decompile<ICursor, int>((cursor, args, keys) =>
            {
                var arr = new int[] {1, 2, 3};
                return arr.Length;
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
        }
    }
}