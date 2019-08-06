using ICSharpCode.Decompiler.CSharp.Syntax;

namespace RedSharper.CSharp
{
    class DecompilationResult
    {
        public AstNode Body { get; }

        public string CursorVariableName { get; }

        public string ArgumentsVariableName { get; }

        public string KeysVariableName { get; }

        public string[] ArgumentsTupleSubKeys { get; }

        public bool ArgumentsAreTuple => ArgumentsTupleSubKeys != null;

        public DecompilationResult(
            AstNode body,
            string cursorVariableName,
            string argumentsVariableName,
            string keysVariableName,
            string[] argumentsTupleSubKeys)
        {
            Body = body;
            CursorVariableName = cursorVariableName;
            ArgumentsVariableName = argumentsVariableName;
            KeysVariableName = keysVariableName;
            ArgumentsTupleSubKeys = argumentsTupleSubKeys;
        }
    }
}