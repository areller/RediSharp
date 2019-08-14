using System.Reflection;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace RediSharp.CSharp
{
    class DecompilationResult
    {
        public Assembly DecompilationAssembly { get; set; }

        public AstNode Body { get; }

        public string CursorVariableName { get; }

        public string ArgumentsVariableName { get; }

        public string KeysVariableName { get; }

        public string[] ArgumentsTupleSubKeys { get; }

        public bool ArgumentsAreTuple => ArgumentsTupleSubKeys != null;

        public DecompilationResult(
            Assembly decompilationAssembly,
            AstNode body,
            string cursorVariableName,
            string argumentsVariableName,
            string keysVariableName,
            string[] argumentsTupleSubKeys)
        {
            DecompilationAssembly = decompilationAssembly;
            Body = body;
            CursorVariableName = cursorVariableName;
            ArgumentsVariableName = argumentsVariableName;
            KeysVariableName = keysVariableName;
            ArgumentsTupleSubKeys = argumentsTupleSubKeys;
        }
    }
}