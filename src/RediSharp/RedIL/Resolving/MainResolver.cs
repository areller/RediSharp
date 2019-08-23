using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ICSharpCode.Decompiler.TypeSystem;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Types;

namespace RediSharp.RedIL.Resolving
{
    class MainResolver
    {
        #region Internal

        class Constructor
        {
            public string[] Signature { get; set; }

            public RedILObjectResolver Resolver { get; set; }
        }
        
        class ProxyDefinition
        {
            public Dictionary<string, int> ProxyTypeParametersOrdinal { get; set; }

            public Dictionary<int, List<Constructor>> Constructors { get; set; }
        }
        
        private Dictionary<string, ProxyDefinition> _proxies;
        
        public MainResolver()
        {
            _proxies = new Dictionary<string, ProxyDefinition>();
            DefineResolvers();
        }
        
        public void AddPack(Dictionary<Type, Type> pack)
        {
            foreach (var item in pack)
            {
                AddResolver(item.Key, item.Value);
            }
        }
        
        /*RedILResolver resolver;
                if (redILResolveAttribute is null)
                {
                    resolver = _compiler._externalResolvers.FindResolver(resolveResult.TargetResult.Type.ReflectionName,
                        resolveResult.TargetResult.Type.FullName, memberReference.MemberName,
                        EntryType.Method);

                    if (resolver is null)
                    {
                        throw new RedILException($"Could not find resolver for '{memberReference.MemberName}' of '{resolveResult.TargetResult.Type.ReflectionName}'");
                    }
                }
                else
                {
                    var resolverTypeArg = redILResolveAttribute.ConstructorArguments.First().Value;
                    var resolverCustomArgs =
                        (redILResolveAttribute.ConstructorArguments.Skip(1).First().Value as
                            ReadOnlyCollection<CustomAttributeTypedArgument>).Select(arg => arg.Value).ToArray();
                    var resolve = Activator.CreateInstance(redILResolveAttribute.AttributeType, resolverTypeArg, resolverCustomArgs) as RedILResolve;
                    resolver = resolve.CreateResolver();
                }*/
        
        public void AddResolver(Type type, Type proxy)
        {
            var typeInfo = type.GetTypeInfo();
            var proxyTypeInfo = proxy.GetTypeInfo();

            if (typeInfo.GenericTypeParameters.Length != proxyTypeInfo.GenericTypeParameters.Length)
            {
                throw new Exception("Base type and proxy must have the same type parameters");
            }

            var proxyTypeParams = Enumerable.Range(0, proxyTypeInfo.GenericTypeParameters.Length)
                .Select(i => (i, proxyTypeInfo.GenericTypeParameters[i]))
                .ToDictionary(p => p.Item2.Name, p => p.i);

            var constructors = new List<Constructor>();
            foreach (var ctor in proxy.GetConstructors())
            {
                var resolveAttr = ctor.GetCustomAttributes();
            }
            
            _proxies.Add(type.FullName, new ProxyDefinition()
            {
                ProxyTypeParametersOrdinal = proxyTypeParams
            });
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
            string name;
            string[] typeParams;
            
            if (type is ParameterizedType)
            {
                
            }
            
            return null;
        }
        
        #endregion

        private void DefineResolvers()
        {
            #region Add Resolvers Here
            
            AddPack(ArrayResolverPacks.GetMapToProxy());
            AddPack(DatabaseResolverPack.GetMapToProxy());
            AddPack(DictionaryResolverPack.GetMapToProxy());
            AddPack(HashEntryResolverPack.GetMapToProxy());
            AddPack(KeyValuePairResolverPack.GetMapToProxy());
            AddPack(ListResolverPack.GetMapToProxy());
            AddPack(NullableResolverPack.GetMapToProxy());
            AddPack(RedisKeyResolverPack.GetMapToProxy());
            AddPack(RedisValueResolverPack.GetMapToProxy());
            AddPack(StringResolverPack.GetMapToProxy());
            AddPack(TimeSpanResolverPack.GetMapToProxy());
            AddPack(DateTimeResolverPack.GetMapToProxy());
            
            #endregion
        }
    }
}