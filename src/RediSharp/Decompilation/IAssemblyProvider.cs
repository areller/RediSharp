using System.Reflection;
using ICSharpCode.Decompiler.Metadata;

namespace RediSharp.Decompilation
{
    interface IAssemblyProvider : IAssemblyResolver
    {
        void Prepare(Assembly assembly);
    }
}