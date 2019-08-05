using System.Linq;
using System.Text;
using RedSharper.RedIL;

namespace RedSharper.Lua
{
    class CompilationInstance
    {
        class CompilationState
        {
            private int _identation;

            private int _currentLine;

            public StringBuilder Builder { get; }

            public CompilationState()
            {
                _currentLine = 0;
                _identation = 0;
                Builder = new StringBuilder();
            }

            public void Ident()
            {
                _identation++;
            }

            public void FinishIdent()
            {
                _identation--;
            }

            public void NewLine()
            {
                Builder.AppendLine();
                _currentLine = 0;
            }

            public void Write(string text)
            {
                if (_currentLine == 0)
                {
                    for (int i = 0; i < _currentLine; i++) Builder.Append(" ");
                }

                Builder.Append(text);
                _currentLine++;
            }
        }

        class Visitor : IRedILVisitor<CompilationState>
        {
            public void VisitAssignNode(AssignNode assign, CompilationState state)
            {
                assign.Left.AcceptVisitor(this, state);
                state.Write(" = ");
                assign.Right.AcceptVisitor(this, state);
            }

            public void VisitBlockNode(BlockNode block, CompilationState state)
            {
                foreach (var child in block.Children)
                {
                    child.AcceptVisitor(this, state);
                    state.NewLine();
                }
            }

            public void VisitIfNode(IfNode @if, CompilationState state)
            {
                state.Write("if ");
                @if.Condition.AcceptVisitor(this, state);
                state.Write(" then");
                state.NewLine();
                state.Ident();

                @if.IfTrue.AcceptVisitor(this, state);
                state.NewLine();
                state.FinishIdent();

                if (@if.IfFalse == null)
                {
                    state.Write("end");
                }
                else
                {
                    state.Write("else");
                    state.NewLine();
                    state.Ident();
                    @if.IfFalse.AcceptVisitor(this, state);
                }
            }

            public void VisitVariableDeclareNode(VariableDeclareNode variableDeclare, CompilationState state)
            {
                state.Builder.Append($"local {variableDeclare.Name} = ");
                variableDeclare.Value.AcceptVisitor(this, state);
            }
        }

        private RedILNode _root;

        private CompilationState _state;

        private IRedILVisitor<CompilationState> _visitor;

        public CompilationInstance(RedILNode root)
        {
            _root = root;
            _state = new CompilationState();
            _visitor = new Visitor();
        }

        public string Compile()
        {
            _root.AcceptVisitor(_visitor, _state);
            return _state.Builder.ToString();
        }
    }
}