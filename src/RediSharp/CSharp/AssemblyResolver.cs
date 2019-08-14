using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.Metadata;

namespace RediSharp.CSharp
{
    class AssemblyResolver : IAssemblyResolver
    {
        private Dictionary<string, Assembly> _references;

        public AssemblyResolver(Assembly mainAssembly)
        {
            _references = new Dictionary<string, Assembly>();
            DFSDependencies(mainAssembly);
        }

        private void DFSDependencies(Assembly mainAssembly)
        {
            var stack = new Stack<Assembly>();
            stack.Push(mainAssembly);

            while (stack.Count > 0)
            {
                var top = stack.Pop();
                if (_references.ContainsKey(top.FullName)) continue;

                _references[top.FullName] = top;

                var refs = top.GetReferencedAssemblies();
                if (refs != null)
                {
                    foreach (var r in refs)
                    {
                        stack.Push(Assembly.Load(r));
                    }
                }
            }
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
                throw new DecompilationException($"Module {moduleName} could not be found");
            }

            return new PEFile(moduleFileName, new FileStream(moduleFileName, FileMode.Open, FileAccess.Read), PEStreamOptions.Default, MetadataReaderOptions.Default);
        }
    }
}