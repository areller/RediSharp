using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ICSharpCode.Decompiler.TypeSystem;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;
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

        class Method
        {
            public string Name { get; set; }

            public string[] Signature { get; set; }

            public RedILMethodResolver Resolver { get; set; }
        }

        class Member
        {
            public string Name { get; set; }

            public RedILMemberResolver Resolver { get; set; }
        }
        
        class TypeDefinition
        {
            public DataValueType DataType { get; set; }

            public Dictionary<string, int> ParametersOrdinal { get; set; }

            public Dictionary<int, List<Constructor>> Constructors { get; set; }

            public Dictionary<(string, int), List<Method>> InstanceMethods { get; set; }

            public Dictionary<string, Member> InstanceMembers { get; set; }

            public Dictionary<(string, int), List<Method>> StaticMethods { get; set; }

            public Dictionary<string, Member> StaticMembers { get; set; }
        }
        
        private Dictionary<string, TypeDefinition> _typeDefs;
        
        public MainResolver()
        {
            _typeDefs = new Dictionary<string, TypeDefinition>();
            DefineResolvers();
        }
        
        public void AddPack(Dictionary<Type, Type> pack)
        {
            foreach (var item in pack)
            {
                AddResolver(item.Key, item.Value);
            }
        }
        
        public void AddResolver(Type type, Type proxy)
        {
            var typeInfo = type.GetTypeInfo();
            var proxyTypeInfo = proxy.GetTypeInfo();

            if (typeInfo.GenericTypeParameters.Length != proxyTypeInfo.GenericTypeParameters.Length)
            {
                throw new Exception("Base type and proxy must have the same type parameters");
            }

            var paramsOrdinal = Enumerable.Range(0, proxyTypeInfo.GenericTypeParameters.Length)
                .Select(i => (i, proxyTypeInfo.GenericTypeParameters[i]))
                .ToDictionary(p => p.Item2.Name, p => p.i);

            var dataTypeAttr = proxy.GetCustomAttributes()
                .FirstOrDefault(attr => attr is RedILDataType) as RedILDataType;
            var dataType = DataValueType.Unknown;
            if (!(dataTypeAttr is null))
            {
                dataType = dataTypeAttr.Type;
            }

            var constructors = new List<Constructor>();
            var instanceMethods = new List<Method>();
            var instanceMembers = new List<Member>();
            var staticMethods = new List<Method>();
            var staticMembers = new List<Member>();

            foreach (var ctor in proxy.GetConstructors())
            {
                var resolveAttr = ctor.GetCustomAttributes()
                    .FirstOrDefault(attr => attr is RedILResolve) as RedILResolve;

                if (resolveAttr is null)
                {
                    continue;
                }

                var signature = ctor.GetParameters()
                    .Select(param => param.ParameterType.ToString()).ToArray();

                var resolver = resolveAttr.CreateObjectResolver();
                constructors.Add(new Constructor()
                {
                    Signature = signature,
                    Resolver = resolver
                });
            }

            foreach (var method in proxy.GetMethods())
            {
                var resolveAttr = method.GetCustomAttributes()
                    .FirstOrDefault(attr => attr is RedILResolve) as RedILResolve;

                if (resolveAttr is null)
                {
                    continue;
                }

                var signature = method.GetParameters()
                    .Select(param => param.ParameterType.ToString()).ToArray();

                var resolver = resolveAttr.CreateMethodResolver();
                (method.IsStatic ? staticMethods : instanceMethods).Add(new Method()
                {
                    Name = method.Name,
                    Signature = signature,
                    Resolver = resolver
                });
            }

            foreach (var member in proxy.GetProperties())
            {
                var resolveAttr = member.GetCustomAttributes()
                    .FirstOrDefault(attr => attr is RedILResolve) as RedILResolve;

                if (resolveAttr is null)
                {
                    continue;
                }

                var resolver = resolveAttr.CreateMemberResolver();
                (member.GetAccessors().First().IsStatic ? staticMembers : instanceMembers).Add(new Member()
                {
                    Name = member.Name,
                    Resolver = resolver
                });
            }

            if (proxy.IsEnum)
            {
                foreach (var enumName in proxy.GetEnumNames())
                {
                    var field = proxy.GetField(enumName);
                    var resolveAttr = field?.GetCustomAttributes()
                        ?.FirstOrDefault(attr => attr is RedILResolve) as RedILResolve;

                    if (resolveAttr is null)
                    {
                        continue;
                    }

                    var resolver = resolveAttr.CreateMemberResolver();
                    staticMembers.Add(new Member()
                    {
                        Name = enumName,
                        Resolver = resolver
                    });
                }
            }

            _typeDefs.Add(type.FullName, new TypeDefinition()
            {
                DataType = dataType,
                ParametersOrdinal = paramsOrdinal,
                Constructors = constructors.GroupBy(c => c.Signature.Length).ToDictionary(g => g.Key, g => g.ToList()),
                InstanceMethods = instanceMethods.GroupBy(m => (m.Name, m.Signature.Length)).ToDictionary(g => g.Key, g => g.ToList()),
                InstanceMembers = instanceMembers.ToDictionary(m => m.Name, m => m),
                StaticMethods = staticMethods.GroupBy(m => (m.Name, m.Signature.Length)).ToDictionary(g => g.Key, g => g.ToList()),
                StaticMembers = staticMembers.ToDictionary(m => m.Name, m => m)
            });
        }

        public DataValueType ResolveDataType(IType type)
        {
            var typeDef = FindTypeDefinition(type);
            return typeDef.DataType;
        }
        
        public RedILMemberResolver ResolveMember(bool isStatic, IType type, string member)
        {
            var typeDef = FindTypeDefinition(type);
            Member memberDef;
            if (!(isStatic ? typeDef.StaticMembers : typeDef.InstanceMembers).TryGetValue(member, out memberDef))
            {
                throw new Exception("Could not find matching member");
            }

            return memberDef.Resolver;
        }

        public RedILMethodResolver ResolveMethod(bool isStatic, IType type, string method, IParameter[] parameters)
        {
            var typeDef = FindTypeDefinition(type);
            List<Method> methodsByNameAndSize;
            if (!(isStatic ? typeDef.StaticMethods : typeDef.InstanceMethods).TryGetValue((method, parameters.Length),
                out methodsByNameAndSize))
            {
                throw new Exception("Could not find matching method");
            }

            var matchingMethods = methodsByNameAndSize
                .Where(m => ParametersSignatureMatch(type, typeDef, m.Signature, parameters))
                .ToList();

            if (matchingMethods.Count == 0)
            {
                throw new Exception("Could not find matching method");
            }
            else if (matchingMethods.Count > 1)
            {
                throw new Exception("Found multiple matches for method");
            }

            return matchingMethods.First().Resolver;
        }

        public RedILObjectResolver ResolveConstructor(IType type, IParameter[] parameters)
        {
            var typeDef = FindTypeDefinition(type);
            List<Constructor> ctorsBySigSize;
            if (!typeDef.Constructors.TryGetValue(parameters.Length, out ctorsBySigSize))
            {
                throw new Exception("Could not find matching constructor");
            }

            var matchingCtors = ctorsBySigSize
                .Where(c => ParametersSignatureMatch(type, typeDef, c.Signature, parameters))
                .ToList();

            if (matchingCtors.Count == 0)
            {
                throw new Exception("Could not find matching constructor");
            }
            else if (matchingCtors.Count > 1)
            {
                throw new Exception("Found multiple matches for constructor");
            }

            return matchingCtors.First().Resolver;
        }
        
        #region Private

        private TypeDefinition FindTypeDefinition(IType type)
        {
            string name;
            // Special treatment for arrays
            if (type.Kind == TypeKind.Array)
            {
                name = typeof(Array).FullName;
            }
            else
            {
                name = type.GetDefinition().ReflectionName;
            }
            
            TypeDefinition typeDef;
            if (!_typeDefs.TryGetValue(name, out typeDef))
            {
                var asm = Assembly.Load(type.GetDefinition().ParentModule.FullAssemblyName);
                var loadedType = asm.GetType(name);
                AddResolver(loadedType, loadedType);
                typeDef = _typeDefs[name];
            }

            return typeDef;
        }

        private bool ParametersSignatureMatch(IType type, TypeDefinition typeDef, string[] signature, IParameter[] parameters)
        {
            var replacedSignature = new string[signature.Length];
            for (int i = 0; i < signature.Length; i++)
            {
                int ordinal;
                if (typeDef.ParametersOrdinal.TryGetValue(signature[i], out ordinal))
                {
                    replacedSignature[i] = type.TypeArguments[ordinal].ReflectionName;
                }
                else
                {
                    //TODO: Handle parameterized types inside generics
                    replacedSignature[i] = signature[i];
                }
            }

            return replacedSignature
                .Zip(parameters, (sigParam, param) => (sigParam, param))
                .All(p => p.sigParam == p.param.Type.ReflectionName);
        }
        
        #endregion
        
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
            AddPack(WhenEnumResolverPack.GetMapToProxy());
            
            #endregion
        }
    }
}