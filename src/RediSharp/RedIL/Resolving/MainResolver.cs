using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving
{
    class MainResolver
    {
        public MainResolver()
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
    }
}