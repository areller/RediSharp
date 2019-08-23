using System.Linq;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        
        private static MainResolver _mainResolver;
        
        [ClassInitialize]
        public static void ClassSetup(TestContext ctx)
        {
            _mainResolver.AddResolver(typeof(Foo), typeof(FooProxy));
        }

        [TestMethod]
        public void ShouldResolveConstructor()
        {
            
        }
    }
}