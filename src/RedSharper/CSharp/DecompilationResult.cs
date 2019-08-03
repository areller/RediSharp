using ICSharpCode.Decompiler.CSharp.Syntax;

namespace RedSharper.CSharp
{
    class DecompilationResult
    {
        public SyntaxTree Body { get; set; }

        public string CursorVariableName { get; set; }

        public string ArgumentsVariableName { get; set; }

        public string KeysVariableName { get; set; }
    }
}