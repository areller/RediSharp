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
        OpResult Set(string key, string value);

        OpResult Get(string key);
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

        public void Add<T>(Func<ICursor, T, OpResult> action, T args)
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

            var ast = decompiler.DecompileAsString(new List<EntityHandle>()
            {
                method.Value
            });

            ;
        }

        private void Bar(ICursor c)
        {
            var x = 3;
            var keys = new string[] { "A", "B", "C" };
            foreach (var key in keys)
            {
                c.Set("myKey_" + key, x.ToString());
            }
        }
    }

    class Program
    {
        private static Foo f;

        static void Main(string[] args)
        {
            IServiceCollection services;
            IMvcBuilder mvc;

            f = new Foo();

            Console.WriteLine("A");

            AddLambda(1, 4, 5);
            AddLambda(3, 2, 1);

            for (int i = 0; i < 10; i++)
            {
                AddLambda(i, i + 1, i + 6);
            }

            Task.WaitAll(Enumerable.Range(0, 10)
                .Select(i => Task.Factory.StartNew(RunNew, i))
                .ToArray());
        }

        static void RunNew(object state)
        {
            var i = (int)state;
            AddLambda(i * 100, i * 200, i * 300);
        } 

        static void AddLambda(int a1, int a2, int a3)
        {
            f.Add<(int a, int b, int c)>((cursor, p) => {
                var x = p.a + 2 * p.b + 3 * p.c;
                var keys = new string[] { "A", "B", "C" };
                foreach (var key in keys)
                {
                    cursor.Set("myKey_" + key, "Hello " + x);
                }
                return cursor.Get("isOk");
            }, (a1, a2, a3));
        }

        static void Add(Func<ICursor, (int a, int b, int c), OpResult> action, (int a, int b, int c) args)
        {
            f.Add(action, args);
        }

        static OpResult DoJob(ICursor c, (int a, int b, int c) args)
        {
            var x = 3 * 2;
            var keys = new string[] { "A", "B", "C", "D" };
            foreach (var key in keys)
            {
                c.Set("myKey_" + key, "Hello2 " + x);
            }
            return c.Get("dfs");
        }
    }
}
