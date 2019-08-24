using ICSharpCode.Decompiler.CSharp.Syntax;
using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving
{
    struct Context
    {
        public CSharpCompiler Compiler { get; }

        public RootNode Root { get; }

        public Expression CurrentExpression { get; }

        public Context(CSharpCompiler compiler, RootNode root, Expression currentExpr)
        {
            Compiler = compiler;
            Root = root;
            CurrentExpression = currentExpr;
        }

        public bool IsInsideStatement()
        {
            return (CurrentExpression?.Parent?.NodeType ?? NodeType.Unknown) == NodeType.Statement;
        }

        public bool IsInsideExpression()
        {
            return (CurrentExpression?.Parent?.NodeType ?? NodeType.Unknown) == NodeType.Expression;
        }
    }
}