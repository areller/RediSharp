using System.Linq;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.UnitTests.Resolving
{
    class SomeKeyResolver : RedILMemberResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return (ConstantValueNode) "abc";
        }
    }
    
    class SomeInterfaceGreetingResolver : RedILMethodResolver
    {
        private string _prefix;

        public SomeInterfaceGreetingResolver(object arg)
        {
            _prefix = (string) arg;
        }
        
        public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
        {
            if (caller is null)
            {
                return arguments.First();
            }
            
            return BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat, BinaryExpressionNode.Create(
                    BinaryExpressionOperator.StringConcat, (ConstantValueNode) _prefix,
                    arguments.First()),
                BinaryExpressionNode.Create(BinaryExpressionOperator.StringConcat, (ConstantValueNode) " ", caller));
        }
    }

    public class SomeInterace : ISomeInterface
    {
        class SomeInterfaceConstructorConstructorResolver : RedILObjectResolver
        {
            public override ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements)
            {
                return arguments.First();
            }
        }

        [RedILResolve(typeof(SomeInterfaceConstructorConstructorResolver))]
        public SomeInterace(string name)
        {
            
        }
        
        [RedILResolve(typeof(SomeInterfaceGreetingResolver), "Class")]
        public string Greeting(string name) => default;

        [RedILResolve(typeof(SomeInterfaceGreetingResolver), "Static")]
        public static string StaticGreeting(string name) => default;

        [RedILResolve(typeof(SomeKeyResolver))]
        public static string SomeKey => default;
    }
    
    public interface ISomeInterface
    {
        [RedILResolve(typeof(SomeInterfaceGreetingResolver), "Interface")]
        string Greeting(string name);
    }
}