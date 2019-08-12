using System;
using ICSharpCode.Decompiler.TypeSystem;

namespace RedSharper.RedIL.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    class RedILResolve : Attribute
    {
        public Type ResolverType { get; }

        public object[] Arguments { get; }

        public RedILResolve(Type resolverType, params object[] arguments)
        {
            if (!resolverType.IsSubclassOf(typeof(RedILResolver)))
            {
                throw new RedILException($"Type '{resolverType}' is not a resolver");
            }
            
            ResolverType = resolverType;
            Arguments = arguments;
        }

        public RedILResolver CreateResolver()
        {
            return Activator.CreateInstance(ResolverType, Arguments) as RedILResolver;
        }
    }
}