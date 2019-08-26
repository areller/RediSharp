using ICSharpCode.Decompiler.CSharp.Syntax;
using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving
{
    class Context
    {
        public CSharpCompiler Compiler { get; }

        public RootNode Root { get; }

        public Expression CurrentExpression { get; }

        public BlockNode CurrentBlock { get; }

        public Context(CSharpCompiler compiler, RootNode root, Expression currentExpr, BlockNode currentBlock)
        {
            Compiler = compiler;
            Root = root;
            CurrentExpression = currentExpr;
            CurrentBlock = currentBlock;
        }

        public bool IsInsideStatement()
        {
            return (CurrentExpression?.Parent?.NodeType ?? NodeType.Unknown) == NodeType.Statement;
        }

        public bool IsInsideExpression()
        {
            return (CurrentExpression?.Parent?.NodeType ?? NodeType.Unknown) == NodeType.Expression;
        }

        public bool IsPartOfBlock()
        {
            return (CurrentExpression?.Parent?.NodeType ?? NodeType.Unknown) == NodeType.Statement &&
                   CurrentExpression.Parent is ExpressionStatement;
        }
    }
}