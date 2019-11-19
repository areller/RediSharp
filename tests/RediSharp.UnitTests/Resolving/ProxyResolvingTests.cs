using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RediSharp.RedIL;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.UnitTests.Resolving
{
    [TestClass]
    public class ProxyResolvingTests
    {
        #region Setup   
        
        class ConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                switch (arguments.Length)
                {
                    case 0:
                        return (ConstantValueNode) 0;
                    case 1:
                        return arguments.First();
                    default: return null;
                }
            }
        }

        class NumberAccessResolver : RedILMemberResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode caller)
            {
                return caller;
            }
        }

        class SetNumberResolver : RedILMethodResolver
        {
            public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
            {
                return new AssignNode(caller, arguments.First());
            }
        }
        
        [RedILDataType(DataValueType.Integer)]
        class FooProxy
        {
            [RedILResolve(typeof(ConstructorResolver))]
            public FooProxy()
            {
                
            }

            [RedILResolve(typeof(ConstructorResolver))]
            public FooProxy(int num)
            {
                
            }

            [RedILResolve(typeof(NumberAccessResolver))]
            public int Number { get; private set; }

            [RedILResolve(typeof(SetNumberResolver))]
            public void SetNumber(int num)
            {
                
            }
            
        }
        
        class Foo
        {
            public Foo()
            {
                Number = 0;
            }

            public Foo(int num)
            {
                Number = num;
            }

            public int Number { get; private set; }

            public void SetNumber(int num)
            {
                Number = num;
            }
        }
        
        #endregion
        
        private static CSharpCompiler _csharpCompiler;
        
        [ClassInitialize]
        public static void ClassSetup(TestContext ctx)
        {
            _csharpCompiler = new CSharpCompiler();
            _csharpCompiler.MainResolver.AddResolver(typeof(Foo), typeof(FooProxy));
        }

        [TestMethod]
        public void ShouldResolveConstructor()
        {
            var csharp = DecompilationResult.CreateFromDelegate<NullCursor, bool>((cursor, args, keys) =>
            {
                var foo1 = new Foo();
                var foo2 = new Foo(3);
                return true;
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
            var block = redIL.Body as BlockNode;
            block.Children.Count.Should().Be(3);
            var varDec = block.Children.First() as VariableDeclareNode;
            varDec.Value.Should().BeEquivalentTo(new ConstantValueNode(DataValueType.Integer, 0));
            varDec = block.Children.Skip(1).First() as VariableDeclareNode;
            varDec.Value.Should().BeEquivalentTo(new ConstantValueNode(DataValueType.Integer, 3));
        }

        [TestMethod]
        public void ShouldResolveMember()
        {
            var csharp = DecompilationResult.CreateFromDelegate<NullCursor, int>((cursor, args, keys) =>
            {
                var foo = new Foo(17);
                return foo.Number;
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
            var block = redIL.Body as BlockNode;
            block.Children.Count.Should().Be(2);
            var dec = block.Children.First() as VariableDeclareNode;
            var ret = block.Children.Last() as ReturnNode;
            ret.Value.Should().BeEquivalentTo(new IdentifierNode(dec.Name.ToString(), DataValueType.Integer));
        }

        [TestMethod]
        public void ShouldResolveMethod()
        {
            var csharp = DecompilationResult.CreateFromDelegate<NullCursor, bool>((cursor, args, keys) =>
            {
                var foo = new Foo();
                foo.SetNumber(10);
                return true;
            });
            var redIL = _csharpCompiler.Compile(csharp) as RootNode;
            var block = redIL.Body as BlockNode;
            block.Children.Count.Should().Be(3);
            var assign = block.Children.Skip(1).First() as AssignNode;
            assign.Right.Should().BeEquivalentTo(new ConstantValueNode(DataValueType.Integer, 10));
        }
    }
}