using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RediSharp.Generator.Compilation.RedIL;
using System.Linq;

namespace RediSharp.Generator.Compilation
{
    class CSharpVisitor : CSharpSyntaxVisitor<Node>
    {
        public override Node? VisitBlock(BlockSyntax node) => new BlockNode(node.Statements.Select(s => s.Accept(this)!).ToArray());

        public override Node? 
    }
}