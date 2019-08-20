using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Types;

namespace RediSharp.RedIL.Resolving
{
    class MainResolver
    {
        public MainResolver()
        {
            #region Add Resolvers Here
            
            AddResolver(typeof(ArrayResolver));
            AddResolver(typeof(DictionaryResolver<,>));
            AddResolver(typeof(HashEntryResolver));
            AddResolver(typeof(KeyValuePairResolver<,>));
            AddResolver(typeof(ListResolver<>));
            AddResolver(typeof(ListInterfaceResolver<>));
            AddResolver(typeof(CollectionInterfaceResolver<>));
            AddResolver(typeof(NullableResolverPack.NullableResolver<>));
            AddResolver(typeof(RedisKeyResolver));
            AddResolver(typeof(RedisValueResolver));
            AddResolver(typeof(TimeSpanResolverPack.TimeSpanResolver));
            
            #endregion
        }

        #region Internal
        
        private void AddResolver(Type resolverType)
        {
            
        }
        
        public RedILMemberResolver ResolveMember(bool isStatic, IType type, string member)
        {
            return null;
        }

        public RedILMethodResolver ResolveMethod(bool isStatic, IType type, string method, IParameter[] parameters)
        {
            return null;
        }

        public RedILObjectResolver ResolveConstructor(IType type, IParameter[] parameters)
        {
            return null;
        }
        
        #endregion
    }
}