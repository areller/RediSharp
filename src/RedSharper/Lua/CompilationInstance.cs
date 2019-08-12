using System;
using System.Linq;
using System.Text;
using RedSharper.Enums;
using RedSharper.RedIL;
using RedSharper.RedIL.Enums;

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
                    for (int i = 0; i < _identation; i++) Builder.Append(" ");
                }

                Builder.Append(text);
                _currentLine++;
            }
        }

        class Visitor : IRedILVisitor<bool, CompilationState>
        {
            public bool VisitArgsTableNode(ArgsTableNode node, CompilationState state)
            {
                state.Write("ARGV");
                return true;
            }

            public bool VisitKeysTableNode(KeysTableNode node, CompilationState state)
            {
                state.Write("KEYS");
                return true;
            }

            public bool VisitAssignNode(AssignNode node, CompilationState state)
            {
                node.Left.AcceptVisitor(this, state);
                state.Write(" = ");
                node.Right.AcceptVisitor(this, state);
                return true;
            }

            public bool VisitBinaryExpressionNode(BinaryExpressionNode node, CompilationState state)
            {
                if (node.Operator == BinaryExpressionOperator.NullCoalescing) state.Write("(");
                EncapsulateOrNot(state, node.Left);
                WriteBinaryOperator(state, node.Operator);
                EncapsulateOrNot(state, node.Right);
                if (node.Operator == BinaryExpressionOperator.NullCoalescing) state.Write(")");
                
                return true;
            }

            public bool VisitBlockNode(BlockNode node, CompilationState state)
            {
                WriteLines(state, node.Children.ToArray());
                return true;
            }

            public bool VisitBreakNode(BreakNode node, CompilationState state)
            {
                state.Write("break");
                return true;
            }

            public bool VisitCallRedisMethodNode(CallRedisMethodNode node, CompilationState state)
            {
                state.Write("redis.call('");
                switch (node.Method)
                {
                    case RedisCommand.Get:
                        state.Write("get");
                        break;
                    case RedisCommand.Set:
                        state.Write("set");
                        break;
                    case RedisCommand.HGet:
                        state.Write("hget");
                        break;
                    case RedisCommand.HMGet:
                        state.Write("hmget");
                        break;
                    case RedisCommand.HSet:
                        state.Write("hset");
                        break;
                    default: throw new LuaCompilationException($"Unsupported redis method '{node.Method}'");
                }
                
                state.Write("'");
                WriteArguments(state, node.Arguments, false);
                state.Write(")");

                return true;
            }

            public bool VisitCastNode(CastNode node, CompilationState state)
            {
                switch (node.DataType)
                {
                    case DataValueType.String:
                        state.Write("tostring");
                        break;
                    case DataValueType.Integer:
                    case DataValueType.Float:
                        state.Write("tonumber");
                        break;
                    default: throw new LuaCompilationException($"Unsupported cast to '{node.DataType}'");
                }
                
                state.Write("(");
                node.Argument.AcceptVisitor(this, state);
                state.Write(")");

                return true;
            }

            public bool VisitConditionalExpressionNode(ConditionalExpressionNode node, CompilationState state)
            {
                EncapsulateOrNot(state, node.Condition);
                state.Write(" and ");
                EncapsulateOrNot(state, node.IfYes);
                state.Write(" or ");
                EncapsulateOrNot(state, node.IfNo);
                
                return true;
            }

            public bool VisitConstantValueNode(ConstantValueNode node, CompilationState state)
            {
                switch (node.DataType)
                {
                    case DataValueType.Boolean:
                        var boolVal = (bool) node.Value;
                        state.Write(boolVal ? "true" : "false");
                        break;
                    case DataValueType.Integer:
                    case DataValueType.Float:
                        state.Write(node.Value.ToString());
                        break;
                    case DataValueType.String:
                        state.Write($"\"{node.Value.ToString()}\"");
                        break;
                    default: throw new LuaCompilationException($"Unable to write constant value node with data type '{node.DataType}'");
                }

                return true;
            }

            public bool VisitDoWhileNode(DoWhileNode node, CompilationState state)
            {
                state.Write("repeat");
                state.NewLine();
                state.Ident();
                node.Body.AcceptVisitor(this, state);
                state.NewLine();
                state.FinishIdent();
                state.Write("until (");
                state.Write("not ");
                EncapsulateOrNot(state, node.Condition);
                state.Write(")");

                return true;
            }

            public bool VisitEmptyNode(EmptyNode node, CompilationState state)
            {
                return true;
            }

            public bool VisitIdentifierNode(IdentifierNode node, CompilationState state)
            {
                state.Write(node.Name);
                return true;
            }

            public bool VisitIfNode(IfNode node, CompilationState state)
            {
                state.Write("if ");
                node.Condition.AcceptVisitor(this, state);
                state.Write(" then");
                state.NewLine();
                state.Ident();
                node.IfTrue.AcceptVisitor(this, state);
                state.NewLine();
                state.FinishIdent();

                //TODO: Optimize to elseif if needed
                if (node.IfFalse == null)
                {
                    state.Write("end");
                }
                else
                {
                    state.Write("else");
                    state.NewLine();
                    state.Ident();
                    node.IfFalse.AcceptVisitor(this, state);
                    state.NewLine();
                    state.FinishIdent();
                    state.Write("end");
                }

                return true;
            }

            public bool VisitNilNode(NilNode node, CompilationState state)
            {
                state.Write("nil");
                return true;
            }

            public bool VisitReturnNode(ReturnNode node, CompilationState state)
            {
                state.Write("return ");
                node.Value.AcceptVisitor(this, state);
                state.Write(";");

                return true;
            }

            public bool VisitStatusNode(StatusNode node, CompilationState state)
            {
                switch (node.Status)
                {
                    case Status.Ok:
                        state.Write("{ ");
                        state.Write("ok = ");
                        state.Write("'OK'");
                        state.Write(" }");
                        break;
                    case Status.Error:
                        var errorMsg = node.Error ?? "Error";
                        state.Write("{ ");
                        state.Write("err = ");
                        state.Write($"'{errorMsg}'");
                        state.Write(" }");
                        break;
                }

                return true;
            }

            public bool VisitTableAccessNode(TableKeyAccessNode node, CompilationState state)
            {
                EncapsulateOrNot(state, node.Table);
                state.Write("[");
                node.Key.AcceptVisitor(this, state);
                state.Write("]");

                return true;
            }

            public bool VisitUnaryExpressionNode(UnaryExpressionNode node, CompilationState state)
            {
                WriteUnaryOperator(state, node.Operator);
                node.Operand.AcceptVisitor(this, state);

                return true;
            }

            public bool VisitUniformOperatorNode(UniformOperatorNode node, CompilationState state)
            {
                EncapsulateOrNot(state, node.Children.First());
                foreach (var child in node.Children.Skip(1))
                {
                    WriteBinaryOperator(state, node.Operator);
                    EncapsulateOrNot(state, child);
                }

                return true;
            }

            public bool VisitVariableDeclareNode(VariableDeclareNode node, CompilationState state)
            {
                state.Write($"local {node.Name}");

                if (node.Value != null)
                {
                    state.Write(" = ");
                    node.Value.AcceptVisitor(this, state);
                }
                
                state.Write(";");

                return true;
            }

            public bool VisitWhileNode(WhileNode node, CompilationState state)
            {
                state.Write("while ");
                node.Condition.AcceptVisitor(this, state);
                state.Write(" do");
                state.NewLine();
                state.Ident();
                node.Body.AcceptVisitor(this, state);
                state.NewLine();
                state.FinishIdent();
                state.Write("end");

                return true;
            }

            public bool VisitCallLuaMethodNode(CallLuaMethodNode node, CompilationState state)
            {
                switch (node.Method)
                {
                    case LuaMethod.StringToLower:
                        state.Write("string.lower");
                        break;
                    default:
                        throw new LuaCompilationException($"Unsupported lua method '{node.Method}'");
                }
                
                state.Write("(");
                WriteArguments(state, node.Arguments);
                state.Write(")");

                return true;
            }

            private void WriteLines(CompilationState state, RedILNode[] lines, bool firstLine = true)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 0 || !firstLine)
                    {
                        state.NewLine();
                    }

                    lines[i].AcceptVisitor(this, state);
                }
            }

            private void WriteArguments(CompilationState state, ExpressionNode[] arguments, bool firstArgument = true)
            {
                for (var i = 0; i < arguments.Length; i++)
                {
                    if (i > 0 || !firstArgument)
                    {
                        state.Write(", ");
                    }

                    arguments[i].AcceptVisitor(this, state);
                }
            }

            private void EncapsulateOrNot(CompilationState state, ExpressionNode expr)
            {
                bool encapsulate = expr is BinaryExpressionNode ||
                                   (expr is UnaryExpressionNode &&
                                    ((UnaryExpressionNode) expr).Operator != UnaryExpressionOperator.Minus) ||
                                   expr is UniformOperatorNode;

                if (encapsulate) state.Write("(");
                expr.AcceptVisitor(this, state);
                if (encapsulate) state.Write(")");
            }

            private void WriteBinaryOperator(CompilationState state, BinaryExpressionOperator op)
            {
                switch (op)
                {
                    case BinaryExpressionOperator.StringConcat:
                        state.Write("..");
                        break;
                    case BinaryExpressionOperator.Add:
                        state.Write("+");
                        break;
                    case BinaryExpressionOperator.Subtract:
                        state.Write("-");
                        break;
                    case BinaryExpressionOperator.Multiply:
                        state.Write("*");
                        break;
                    case BinaryExpressionOperator.Divide:
                        state.Write("/'");
                        break;
                    case BinaryExpressionOperator.Modulus:
                        state.Write("%");
                        break;
                    case BinaryExpressionOperator.Equal:
                        state.Write("==");
                        break;
                    case BinaryExpressionOperator.Less:
                        state.Write("<");
                        break;
                    case BinaryExpressionOperator.Greater:
                        state.Write(">");
                        break;
                    case BinaryExpressionOperator.NotEqual:
                        state.Write("~=");
                        break;
                    case BinaryExpressionOperator.LessEqual:
                        state.Write("<=");
                        break;
                    case BinaryExpressionOperator.GreaterEqual:
                        state.Write(">=");
                        break;
                    case BinaryExpressionOperator.Or:
                        state.Write("or");
                        break;
                    case BinaryExpressionOperator.And:
                        state.Write("and");
                        break;
                    case BinaryExpressionOperator.NullCoalescing:
                        state.Write(" or ");
                        break;
                }
            }

            private void WriteUnaryOperator(CompilationState state, UnaryExpressionOperator op)
            {
                switch (op)
                {
                    case UnaryExpressionOperator.Minus:
                        state.Write("-");
                        break;
                    case UnaryExpressionOperator.Not:
                        state.Write("not ");
                        break;
                }
            }
        }

        private RedILNode _root;

        private CompilationState _state;

        private IRedILVisitor<bool, CompilationState> _visitor;

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