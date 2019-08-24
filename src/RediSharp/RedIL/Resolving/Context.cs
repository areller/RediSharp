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
    }
}