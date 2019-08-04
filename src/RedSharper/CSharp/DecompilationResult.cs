using ICSharpCode.Decompiler.CSharp.Syntax;

namespace RedSharper.CSharp
{
    class DecompilationResult
    {
        public AstNode Body { get; }

        public string CursorVariableName { get; }

        public string ArgumentsVariableName { get; }

        public string KeysVariableName { get; }

        public bool ArgumentsAreTuple { get; }

        public string[] ArgumentsTupleSubKeys { get; }

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
            ArgumentsAreTuple = argumentsTupleSubKeys != null;
            ArgumentsTupleSubKeys = argumentsTupleSubKeys;
        }
    }
}