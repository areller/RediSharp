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

        interface ITypeAdapter
        {
            bool IsTypeParameter();

            bool IsArray();

            bool IsGeneric();

            string Name { get; }

            IList<ITypeAdapter> TypeArguments { get; }

            ITypeAdapter ElementType { get; }
        }

        class ReflectionTypeAdapter : ITypeAdapter
        {
            private Type _type;
            
            public ReflectionTypeAdapter(Type type)
            {
                _type = type;
            }

            public bool IsTypeParameter() => _type.IsGenericParameter;

            public bool IsArray() => _type.IsArray;

            public bool IsGeneric() => _type.IsGenericType;

            public string Name => IsTypeParameter() ? _type.Name : _type.FullName;

            public IList<ITypeAdapter> TypeArguments =>
                _type.GenericTypeArguments.Select(arg => new ReflectionTypeAdapter(arg)).ToArray();

            public ITypeAdapter ElementType => new ReflectionTypeAdapter(_type.GetElementType());
        }

        class ILSpyTypeAdapter : ITypeAdapter
        {
            private IType _type;
            
            public ILSpyTypeAdapter(IType type)
            {
                _type = type;
            }

            public bool IsTypeParameter() => _type.Kind == TypeKind.TypeParameter;

            public bool IsArray() => _type.Kind == TypeKind.Array && _type is ArrayType;

            public bool IsGeneric() => _type.TypeArguments.Count > 0;

            public string Name => IsTypeParameter() ? _type.Name : _type.FullName;

            public IList<ITypeAdapter> TypeArguments =>
                _type.TypeArguments.Select(arg => new ILSpyTypeAdapter(arg)).ToArray();

            public ITypeAdapter ElementType => new ILSpyTypeAdapter(((ArrayType)_type).ElementType);
        }

        class Constructor
        {
            public Type[] Signature { get; set; }

            public RedILObjectResolver Resolver { get; set; }
        }

        class Method
        {
            public string Name { get; set; }

            public Type[] Signature { get; set; }

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

            public RedILValueResolver ValueResolver { get; set; }

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

            RedILValueResolver valueResolver = null;
            var valueResolveAttr = proxy.GetCustomAttributes()
                .FirstOrDefault(attr => attr is RedILResolve) as RedILResolve;
            if (!(valueResolveAttr is null))
            {
                valueResolver = valueResolveAttr.CreateValueResolver();
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
                    .Select(param => param.ParameterType).ToArray();

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
                    .Select(param => param.ParameterType).ToArray();

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
                ValueResolver = valueResolver,
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

        public RedILValueResolver ResolveValue(IType type)
        {
            var typeDef = FindTypeDefinition(type);
            return typeDef.ValueResolver;
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

        private bool ParametersSignatureMatch(IType type, TypeDefinition typeDef, Type[] signature, IParameter[] parameters)
        {
            /*
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
                .All(p => p.sigParam == FixParamReflectionName(p.param.Type.ReflectionName));*/

            return signature.Length == parameters.Length &&
                   signature.Zip(parameters, (s, p) => (s, p))
                       .All(p => TypesEqual(type, typeDef, new ReflectionTypeAdapter(p.s),
                           new ILSpyTypeAdapter(p.p.Type)));
        }

        private bool TypesEqual(IType classType, TypeDefinition typeDef, ITypeAdapter sigType, ITypeAdapter paramType)
        {
            if (sigType.IsTypeParameter())
            {
                sigType = new ILSpyTypeAdapter(classType.TypeArguments[typeDef.ParametersOrdinal[sigType.Name]]);
            }

            if (sigType.IsArray())
            {
                return paramType.IsArray() &&
                       TypesEqual(classType, typeDef, sigType.ElementType, paramType.ElementType);
            }
            else if (sigType.IsGeneric())
            {
                return paramType.IsGeneric() && sigType.TypeArguments.Count == paramType.TypeArguments.Count && sigType
                           .TypeArguments.Zip(paramType.TypeArguments, (s, p) => (s, p))
                           .All(p => TypesEqual(classType, typeDef, p.s, p.p));
            }
            else
            {
                return sigType.Name == paramType.Name;
            }
        }

        private string FixParamReflectionName(string paramReflectionName)
        {
            //TODO: Check if it's enough
            return paramReflectionName.Replace("[[", "[").Replace("]]", "]");
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
            AddPack(CommandFlagsResolverPack.GetMapToProxy());
            
            #endregion
        }
    }
}