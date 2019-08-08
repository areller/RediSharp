using System.Linq;
using ICSharpCode.Decompiler.CSharp.Syntax;
using RedSharper.CSharp;
using Newtonsoft.Json;

namespace RedSharper.RedIL
{
    class CSharpCompiler
    {
        public RedILNode Compile(DecompilationResult csharp)
        {
            return Visit(csharp.Body, csharp);
        }

        private RedILNode Visit(AstNode node, DecompilationResult csharp)
        {
            switch (node)
            {
                case BlockStatement blockStmt: return VisitBlockStatement(blockStmt, csharp);
                case VariableDeclarationStatement varDecStmt: return VisitVariableDeclarationStatement(varDecStmt, csharp);
                default: throw new RedILException($"Unsupported AstNode of type '{node.NodeType}'");
            }
        }

        private RedILNode VisitBlockStatement(BlockStatement blockStmt, DecompilationResult csharp)
            => new BlockNode(blockStmt.Children.Select(child => Visit(child, csharp)).ToList());

        private RedILNode VisitVariableDeclarationStatement(VariableDeclarationStatement varDecStmt, DecompilationResult csharp)
        {
            return null;
        }
    }
}