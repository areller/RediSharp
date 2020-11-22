using System;

namespace RediSharp.Generator.Resolving
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    class ProxyTypeAttribute : Attribute
    {
        public Type? OriginalType { get; }
        public string Namespace { get; }
        public string TypeLit { get; }

        public ProxyTypeAttribute(Type originalType)
        {
            OriginalType = originalType;
            Namespace = originalType.Namespace;
            TypeLit = originalType.Name;
        }

        public ProxyTypeAttribute(string ns, string typeLit)
        {
            Namespace = ns;
            TypeLit = typeLit;
        }

        public ProxyTypeAttribute(string ns, Type originalType)
        {
            OriginalType = originalType;
            Namespace = ns;
            TypeLit = originalType.Name;
        }
    }
}
