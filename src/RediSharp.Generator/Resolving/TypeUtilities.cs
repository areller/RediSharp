using Microsoft.CodeAnalysis;
using System;

namespace RediSharp.Generator.Resolving
{
    public static class TypeUtilities
    {
        public static bool SymbolSameAsType(ISymbol symbol, Type type)
        {
            if (SymbolSameAsTypeDirect(symbol, type, type.Namespace, type.Name))
                return true;

            foreach (var attr in type.GetCustomAttributes(typeof(ProxyTypeAttribute), false))
            {
                if (attr is ProxyTypeAttribute proxyAttr)
                {
                    if (SymbolSameAsTypeDirect(symbol, type, proxyAttr.Namespace, proxyAttr.TypeLit))
                        return true;
                }
            }

            return false;
        }

        private static bool SymbolSameAsTypeDirect(ISymbol symbol, Type type, string ns, string typeLit)
        {
            return symbol is INamedTypeSymbol namedTypeSymbol &&
                namedTypeSymbol.ContainingNamespace.ToDisplayString() == ns &&
                namedTypeSymbol.MetadataName == typeLit;
        }
    }
}