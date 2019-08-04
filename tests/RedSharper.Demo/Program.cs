using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.CSharp;
using System.IO;
using System.Reflection.PortableExecutable;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RedSharper.Contracts;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace RedSharper.Demo
{
    class CustomAssemblyResolver : IAssemblyResolver
    {
        private Dictionary<string, Assembly> _references;

        public CustomAssemblyResolver(Assembly asm)
        {
            _references = new Dictionary<string, Assembly>();
            var stack = new Stack<Assembly>();
            stack.Push(asm);

            while (stack.Count > 0)
            {
                var top = stack.Pop();
                if (_references.ContainsKey(top.FullName)) continue;

                _references[top.FullName] = top;

                var refs = top.GetReferencedAssemblies();
                if (refs != null && refs.Length > 0)
                {
                    foreach (var r in refs)
                    {
                        stack.Push(Assembly.Load(r));
                    }
                }
            }

            ;
        }

        public PEFile Resolve(IAssemblyReference reference)
        {
            var asm = _references[reference.FullName];
            var file = asm.Location;
            return new PEFile(file, new FileStream(file, FileMode.Open, FileAccess.Read), PEStreamOptions.Default, MetadataReaderOptions.Default);
        }

        public PEFile ResolveModule(PEFile mainModule, string moduleName)
        {
            var baseDir = Path.GetDirectoryName(mainModule.FileName);
            var moduleFileName = Path.Combine(baseDir, moduleName);
            if (!File.Exists(moduleFileName))
            {
                throw new Exception($"Module {moduleName} could not be found");
            }
            return new PEFile(moduleFileName, new FileStream(moduleFileName, FileMode.Open, FileAccess.Read), PEStreamOptions.Default, MetadataReaderOptions.Default);
        }
    }

    struct OpResult
    {
        public bool IsOk { get; set; }

        public object Value { get; set; }

        public static implicit operator string(OpResult result)
        {
            return result.Value.ToString();
        }
    }

    interface ICursor
    {
        RedResult Set(string key, string value);

        RedSingleResult Get(string key);
    }

    class Foo
    {
        private HashSet<object> _set;

        public int c;

        public Foo()
        {
            c = 0;
            _set = new HashSet<object>();
        }

        public void Add<T>(Func<ICursor, string[], T, RedResult> action, T args, string[] keys = null)
            where T : struct, ITuple
        {
            //var file = "/Users/areller/playground/redsharper/tests/RedSharper.Demo/bin/Debug/netcoreapp2.2/RedSharper.Demo.dll";
            var asm = Assembly.GetCallingAssembly();
            var file = asm.Location;
            var resolver = new CustomAssemblyResolver(asm);
            var decompiler = new CSharpDecompiler(file, resolver, new DecompilerSettings());

            var token = action.Method.MetadataToken;
            //var token = typeof(Foo).GetMethod("Add").MetadataToken;
            var method = MetadataTokenHelpers.TryAsEntityHandle(token);

            var ast = decompiler.Decompile(new List<EntityHandle>()
            {
                method.Value
            });

            ;
        }

        public void AddTwo<T>(Func<ICursor, string[], T, int> action)
            where T : struct, ITuple
        {
            var asm = Assembly.GetCallingAssembly();
            var file = asm.Location;
            var resolver = new CustomAssemblyResolver(asm);
            var decompiler = new CSharpDecompiler(file, resolver, new DecompilerSettings());

            var token = action.Method.MetadataToken;
            //var token = typeof(Foo).GetMethod("Add").MetadataToken;
            var method = MetadataTokenHelpers.TryAsEntityHandle(token);

            var ast = decompiler.Decompile(new List<EntityHandle>()
            {
                method.Value
            });

            ;
        }
    }

    class Program
    {
        private static Foo f;

        static void Main(string[] args)
        {
            Role role;
            
            NodeType type;
            IServiceCollection services;
            IMvcBuilder mvc;

            f = new Foo();

            f.Add<(int a, int b, int c)>((cursor, k, p) => {
                var x = 3;
                x++;
                (int a, int b, int c) = p;
                if (a > 0 && a < 5)
                {
                    cursor.Set("aa", "a");
                }
                else if (a >= 5 && a < 10)
                {
                    cursor.Set("aa", "b");
                }
                else
                {
                    cursor.Set("aa", "c");
                }
                return cursor.Set("ok", c.ToString());
            }, (1, 2, 3));

            f.AddTwo<(int a, int b, int c)>((cursor, k, p) => {
                var x = p.a + 2 * p.b + 3 * p.c;
                var keys = new string[] { "A", "B", "C" };
                foreach (var key in keys)
                {
                    cursor.Set("myKey_" + key, "Hello " + x);
                }
                cursor.Get("isOk");
                return 0;
            });
        }
    }
}
