using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;
using ICSharpCode.Decompiler.Metadata;

namespace RediSharp.CSharp
{
    class AssemblyResolver : IAssemblyResolver
    {
        private ReaderWriterLockSlim _locker;
        
        private Dictionary<string, Assembly> _references;

        private Dictionary<string, Assembly> _referencesByName;
        
        public AssemblyResolver(Assembly rootAssembly, Assembly callingAssembly)
        {
            _locker = new ReaderWriterLockSlim();
            _references = new Dictionary<string, Assembly>();
            _referencesByName = new Dictionary<string, Assembly>();
            
            /*
             * This is for cases when the root assembly loads the calling assembly dynamically.
             * Is such case, the calling assembly won't appear inside the root's assembly dependencies graph
             * We don't want to load only the calling assembly, since the initialization of the RediSharp client might happen in a referencing assembly (referencing the calling assembly)
             */
            
            DFSDependencies(callingAssembly);
            DFSDependencies(rootAssembly);
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
                _referencesByName[top.GetName().Name] = top;

                var refs = top.GetReferencedAssemblies();
                if (!(refs is null))
                {
                    foreach (var r in refs)
                    {
                        try
                        {
                            stack.Push(Assembly.Load(r));
                        }
                        catch (FileNotFoundException)
                        {
                            // TODO: Either 1) find how to check whether an assembly is loadable beforehand. 2) Load only specific assemblies (probably better) 
                        }
                    }
                }
            }
        }

        public void LoadAssembly(Assembly assembly)
        {
            bool exists;
            _locker.EnterReadLock();
            try
            {
                exists = _references.ContainsKey(assembly.FullName);
            }
            finally
            {
                _locker.ExitReadLock();
            }

            if (!exists)
            {
                _locker.EnterWriteLock();
                try
                {
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
                Assembly asm;
                if (!_references.TryGetValue(reference.FullName, out asm))
                {
                    if (!_referencesByName.TryGetValue(reference.Name, out asm))
                    {
                        throw new DecompilationException($"Reference {reference.Name} could not be found");
                    }
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
                throw new DecompilationException($"Module {moduleName} could not be found");
            }

            return new PEFile(moduleFileName, new FileStream(moduleFileName, FileMode.Open, FileAccess.Read), PEStreamOptions.Default, MetadataReaderOptions.Default);
        }
    }
}