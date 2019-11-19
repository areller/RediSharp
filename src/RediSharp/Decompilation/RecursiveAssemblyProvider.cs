using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;
using ICSharpCode.Decompiler.Metadata;

namespace RediSharp.Decompilation
{
    class RecursiveAssemblyProvider : IAssemblyProvider
    {
        private ReaderWriterLockSlim _locker;

        private Dictionary<string, Assembly> _assemblies;

        private bool _throwOnMissing;

        public RecursiveAssemblyProvider(bool throwOnMissing)
        {
            _locker = new ReaderWriterLockSlim();
            _assemblies = new Dictionary<string, Assembly>();
            _throwOnMissing = throwOnMissing;
        }

        private void DFSDependencies(Assembly mainAssembly)
        {
            var stack = new Stack<Assembly>();
            stack.Push(mainAssembly);
            
            while (stack.Count > 0)
            {
                var top = stack.Pop();
                if (_assemblies.ContainsKey(top.FullName)) continue;

                _assemblies[top.FullName] = top;

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

        public void Prepare(Assembly assembly)
        {
            bool write;
            _locker.EnterReadLock();
            try
            {
                write = !_assemblies.ContainsKey(assembly.FullName);
            }
            finally
            {
                _locker.ExitReadLock();
            }

            if (write)
            {
                _locker.EnterWriteLock();
                try
                {
                    if (_assemblies.ContainsKey(assembly.FullName)) return;
                    DFSDependencies(assembly);
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
        }

        public PEFile Resolve(IAssemblyReference reference)
        {
            _locker.EnterReadLock();
            try
            {
                if (!_assemblies.TryGetValue(reference.FullName, out Assembly asm))
                {
                    if (_throwOnMissing)
                        throw new AssemblyResolvingException(reference.FullName);
                    else
                        return null;
                }

                var file = asm.Location;
                return new PEFile(file, new FileStream(file, FileMode.Open, FileAccess.Read), PEStreamOptions.Default,
                    MetadataReaderOptions.Default);
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        public PEFile ResolveModule(PEFile mainModule, string moduleName)
        {
            var baseDir = Path.GetDirectoryName(mainModule.FileName);
            var moduleFileName = Path.Combine(baseDir, moduleName);
            if (!File.Exists(moduleFileName))
            {
                if (_throwOnMissing)
                    throw new AssemblyResolvingException(moduleName);
                else
                    return null;
            }

            return new PEFile(moduleFileName, new FileStream(moduleFileName, FileMode.Open, FileAccess.Read), PEStreamOptions.Default,
                MetadataReaderOptions.Default);
        }
    }
}