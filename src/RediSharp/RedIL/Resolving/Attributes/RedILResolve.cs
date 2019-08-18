using System;

namespace RediSharp.RedIL.Resolving.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    class RedILResolve : Attribute
    {
        private byte _resolverEnum;

        private Type _resolverType;

        public object[] Arguments { get; set; }

        public RedILResolve(Type resolverType, params object[] arguments)
        {
            if (resolverType.IsSubclassOf(typeof(RedILMethodResolver)))
            {
                _resolverEnum = 0;
            }
            else if (resolverType.IsSubclassOf(typeof(RedILMemberResolver)))
            {
                _resolverEnum = 1;
            }
            else
            {
                throw new RedILException($"Type '{resolverType}' is not a resolver");
            }

            _resolverType = resolverType;
            Arguments = arguments;
        }

        public RedILMethodResolver CreateMethodResolver()
        {
            if (_resolverEnum != 0)
            {
                throw new RedILException($"Unable to resolve method resolver from member resolver attribute");
            }
            
            return Activator.CreateInstance(_resolverType, Arguments) as RedILMethodResolver;
        }

        public RedILMemberResolver CreateMemberResolver()
        {
            if (_resolverEnum != 1)
            {
                throw new RedILException($"Unable to resolve enum resolver from method resolver attribute");
            }
            
            return Activator.CreateInstance(_resolverType, Arguments) as RedILMemberResolver;
        }
    }
}