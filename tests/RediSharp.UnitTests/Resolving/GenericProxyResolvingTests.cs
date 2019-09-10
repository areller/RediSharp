using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.CSharp;
using RediSharp.RedIL;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.UnitTests.Resolving
{
    [TestClass]
    public class GenericProxyResolvingTests
    {
        #region Setup

        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                return arguments.First();
            }
        }

        class NumberAccessResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return caller;
            }
        }

        class GetNumberResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return caller;
            }
        }

        class FooProxy<T>
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public FooProxy(T val)
            {}

            [RedILResolve(typeof(NumberAccessResolver))]
            public T Number { get; private set; }

            [RedILResolve(typeof(GetNumberResolver))]
            public T GetNumber() => default;
        }
        
        class Foo<T>
        {
            public Foo(T val)
            {
                Number = val;
            }

            public T Number { get; private set; }

            public T GetNumber() => Number;
        }
        
        #endregion

        private static ActionDecompiler _actionDecompiler;

        private static CSharpCompiler _csharpCompiler;

        [ClassInitialize]
        public static void ClassSetup(TestContext ctx)
        {
            _actionDecompiler = new ActionDecompiler(Assembly.GetCallingAssembly());
            _csharpCompiler = new CSharpCompiler();
            _csharpCompiler.MainResolver.AddResolver(typeof(Foo<>), typeof(FooProxy<>));
        }

        [TestMethod]
        public void ShouldResolveConstructor()
        {
            var csharp = _actionDecompiler.Decompile<NullCursor, bool>((cursor, args, keys) =>
            {
                var foo = new Foo<double>(3.5);
                return true;
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
            var block = redIL.Body as BlockNode;
            block.Children.Count.Should().Be(2);
            var varDec = block.Children.First() as VariableDeclareNode;
            varDec.Value.Should().BeEquivalentTo(new ConstantValueNode(DataValueType.Float, (double) 3.5));
        }

        [TestMethod]
        public void ShouldResolveMember()
        {
            var csharp = _actionDecompiler.Decompile<NullCursor, double>((cursor, args, keys) =>
            {
                var foo = new Foo<double>(3.5);
                return foo.Number;
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
            var block = redIL.Body as BlockNode;
            block.Children.Count.Should().Be(2);
            var dec = block.Children.First() as VariableDeclareNode;
            var ret = block.Children.Last() as ReturnNode;
            ret.Value.Type.Should().Be(RedILNodeType.Parameter);
            (ret.Value as IdentifierNode).Name.Should().Be(dec.Name.ToString());
        }

        [TestMethod]
        public void ShouldResolveMethod()
        {
            var csharp = _actionDecompiler.Decompile<NullCursor, double>((cursor, args, keys) =>
            {
                var foo = new Foo<double>(4);
                return foo.GetNumber();
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
            var block = redIL.Body as BlockNode;
            block.Children.Count.Should().Be(2);
            var dec = block.Children.First() as VariableDeclareNode;
            var ret = block.Children.Last() as ReturnNode;
            ret.Value.Type.Should().Be(RedILNodeType.Parameter);
            (ret.Value as IdentifierNode).Name.Should().Be(dec.Name.ToString());
        }
    }
}