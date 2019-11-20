using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using RediSharp.RedIL;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;

namespace RediSharp.Lua
{
    class CompilationInstance
    {
        #region Function Store
        
        private static readonly Dictionary<LuaBuiltinMethod, string> _builtInMethods =
            new Dictionary<LuaBuiltinMethod, string>()
            {
                { LuaBuiltinMethod.StringSub, "string.sub" },
                { LuaBuiltinMethod.StringGSub, "string.gsub" },
                { LuaBuiltinMethod.StringToLower, "string.lower" },
                { LuaBuiltinMethod.StringToUpper, "string.upper" },
                { LuaBuiltinMethod.StringLength, "string.len" },
                { LuaBuiltinMethod.StringFind, "string.find" },
                { LuaBuiltinMethod.TableUnpack, "unpack" },
                { LuaBuiltinMethod.TableInsert, "table.insert" },
                { LuaBuiltinMethod.TableRemove, "table.remove" },
                { LuaBuiltinMethod.TableGetN, "table.getn" },
                { LuaBuiltinMethod.TableConcat, "table.concat" },
                { LuaBuiltinMethod.Type, "type" },
                { LuaBuiltinMethod.JsonEncode, "cjson.encode" },
                { LuaBuiltinMethod.JsonDecode, "cjson.decode" }
            };
        
        private static readonly Dictionary<LuaFunction, string> _functions = new Dictionary<LuaFunction, string>()
        {
            { LuaFunction.TableArrayContains, "local {{func_name}} = function(tbl, elem) for _, v in ipairs(tbl) do if v == elem then return true; end end return false; end" },
            { LuaFunction.TableArrayRemove, "local {{func_name}} = function(tbl, elem) for i, v in ipairs(tbl) do if v == elem then table.remove(tbl, i); return true; end end return false; end" },
            { LuaFunction.TableArrayIndexOf, "local {{func_name} = function(tbl, elem) for i, v in ipairs(tbl) do if v == elem then return i - 1; end end return -1; end" },
            { LuaFunction.TableDictHasKey, "local {{func_name}} = function(tbl, key) return tbl[key] ~= nil end" },
            { LuaFunction.TableDictKeys, "local {{func_name} = function(tbl) local keys = {}; for k, _ in pairs(tbl) do table.insert(keys, k); end return keys; end" },
            { LuaFunction.TableDictValues, "local {{func_name}} = function(tbl) local values = {}; for _, v in pairs(tbl) do table.insert(values, v); end return values; end" },
            { LuaFunction.TableDictRemove, "local {{func_name}} = function(tbl, key) if tbl[key] == nil then return false; end tbl[key] = nil; return true; end" },
            { LuaFunction.TableCount, "local {{func_name}} = function(tbl) local count = 0; for _ in pairs(tbl) do count = count + 1; end return count; end" },
            { LuaFunction.TableClear, "local {{func_name}} = function(tbl) for k, _ in pairs(tbl) do tbl[k] = nil; end end" },
            { LuaFunction.TableDeepUnpack, "local {{func_name}} = function(tbl) local arr = {}; for _, v in ipairs(tbl) do table.insert(arr, v[1]); table.insert(arr, tostring(v[2])); end return unpack(arr); end" },
            { LuaFunction.TableUnpack, "local {{func_name}} = function(tbl) local arr = {}; for _, v in ipairs(tbl) do table.insert(arr, tostring(v)); end return unpack(arr); end" },
            { LuaFunction.TableGroupToKV, "local {{func_name}} = function(tbl) local arr = {}; for i=0,table.getn(tbl)/2-1 do table.insert(arr, {tbl[2*i+1],tbl[2*i+2]}); end return arr; end" },
            { LuaFunction.TableGroupToKVReverse, "local {{func_name}} = function(tbl) local arr = {}; for i=0,table.getn(tbl)/2-1 do table.insert(arr, {tbl[2*i+2],tbl[2*i+1]}); end return arr; end" }
        };
        
        #endregion
        
        class CompilationState
        {
            private int _identation;

            private int _currentLine;

            private int _lastTempId;

            private Dictionary<LuaFunction, string> _functionPointers;

            private Dictionary<string, string> _tempIdentifiers;

            public StringBuilder Builder { get; }

            public List<string> FunctionDefinitions { get; }

            public CompilationState(RootNode root)
            {
                _currentLine = 0;
                _identation = 0;
                Builder = new StringBuilder();

                _lastTempId = 0;
                foreach (var ident in root.Identifiers)
                {
                    if (ident[0] == '_')
                    {
                        if (int.TryParse(ident.Substring(1), out var id))
                        {
                            _lastTempId = Math.Max(_lastTempId, id);
                        }
                    }
                }

                FunctionDefinitions = new List<string>();
                _functionPointers = new Dictionary<LuaFunction, string>();
                _tempIdentifiers = new Dictionary<string, string>();
            }

            public string GetFunctionId(LuaFunction functionName)
            {
                if (!_functionPointers.TryGetValue(functionName, out var pointer))
                {
                    if (!_functions.TryGetValue(functionName, out var functionDefTemplate))
                    {
                        throw new LuaCompilationException($"Could not find lua function '{functionName}'");
                    }

                    pointer = GetNewId();
                    FunctionDefinitions.Add(functionDefTemplate.Replace("{{func_name}}", pointer));
                    _functionPointers.Add(functionName, pointer);
                }

                return pointer;
            }

            public string GetNewId()
            {
                var idNum = Interlocked.Increment(ref _lastTempId);
                return $"_{idNum}";
            }

            public string GetNewId(string tempId)
            {
                if (!_tempIdentifiers.TryGetValue(tempId, out var id))
                {
                    id = GetNewId();
                    _tempIdentifiers.Add(tempId, id);
                }

                return id;
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
                WriteIdent();
                Builder.Append(text);
                _currentLine++;
            }

            public void Write(char ch)
            {
                WriteIdent();
                Builder.Append(ch);
                _currentLine++;
            }

            private void WriteIdent()
            {
                if (_currentLine == 0)
                {
                    for (int i = 0; i < _identation; i++) Builder.Append(" ");
                }
            }
        }

        class Visitor : IRedILVisitor<bool, CompilationState>
        {
            public bool VisitRootNode(RootNode node, CompilationState state)
            {
                foreach (var dec in node.GlobalVariables)
                {
                    dec.AcceptVisitor(this, state);
                    state.NewLine();
                }
                return node.Body.AcceptVisitor(this, state);
            }

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
                node.Caller.AcceptVisitor(this, state);
                state.Write(".pcall(");
                node.Method.AcceptVisitor(this, state);
                WriteArguments(state, node.Arguments, false);
                state.Write(")");

                return true;
            }

            public bool VisitCastNode(CastNode node, CompilationState state)
            {
                var casting = true;
                switch (node.DataType)
                {
                    case DataValueType.String:
                        state.Write("tostring");
                        break;
                    case DataValueType.Integer:
                    case DataValueType.Float:
                        state.Write("tonumber");
                        break;
                    case DataValueType.Boolean:
                        return BooleanCasting(state, node.Argument);
                    default:
                        casting = false;
                        break;
                }

                if (casting) state.Write("(");
                node.Argument.AcceptVisitor(this, state);
                if (casting) state.Write(")");

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
                        WriteString(state, node.Value.ToString());
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

            public bool VisitTemporaryIdentifierNode(TemporaryIdentifierNode node, CompilationState state)
            {
                state.Write(ResolveTemporaryIdentifier(state, node));
                return true;
            }

            public bool VisitIfNode(IfNode node, CompilationState state)
            {
                bool first = true;
                foreach (var stmt in node.Ifs)
                {
                    state.Write(first ? "if " : "elseif ");
                    stmt.Key.AcceptVisitor(this, state);
                    state.Write(" then");
                    state.NewLine();
                    state.Ident();
                    stmt.Value.AcceptVisitor(this, state);
                    state.NewLine();
                    state.FinishIdent();
                    first = false;
                }

                if (!(node.IfElse is null))
                {
                    state.Write("else");
                    state.NewLine();
                    state.Ident();
                    node.IfElse.AcceptVisitor(this, state);
                    state.NewLine();
                    state.FinishIdent();
                }
                
                state.Write("end");
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
                        WriteString(state, "Ok");
                        state.Write(" }");
                        break;
                    case Status.Error:
                        state.Write("{ ");
                        state.Write("err = ");
                        (node.Error ?? new ConstantValueNode(DataValueType.String, "Error")).AcceptVisitor(this, state);
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
                EncapsulateOrNot(state, node.Operand);
                
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
                state.Write("local ");
                if (node.Name.Type == RedILNodeType.Constant && node.Name.DataType == DataValueType.String)
                {
                    state.Write(((ConstantValueNode)node.Name).Value.ToString());
                }
                else if (node.Name.Type == RedILNodeType.TemporaryParameter)
                {
                    state.Write(ResolveTemporaryIdentifier(state, node.Name as TemporaryIdentifierNode));
                }
                else
                {
                    throw new LuaCompilationException($"Cannot accept variable declare node with '{node.Name.Type}' name");
                }

                if (!(node.Value is null))
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

            public bool VisitCallBuiltinLuaMethodNode(CallBuiltinLuaMethodNode node, CompilationState state)
            {
                if (!_builtInMethods.TryGetValue(node.Method, out var method))
                {
                    throw new LuaCompilationException($"Unsupported lua method '{node.Method}'");
                }
                
                state.Write(method);
                state.Write("(");
                WriteArguments(state, node.Arguments);
                state.Write(")");

                return true;
            }

            public bool VisitCallLuaFunctionNode(CallLuaFunctionNode node, CompilationState state)
            {
                var pointer = state.GetFunctionId(node.Name);
                state.Write(pointer);
                state.Write("(");
                WriteArguments(state, node.Arguments);
                state.Write(")");

                return true;
            }

            public bool VisitCursorNode(CursorNode node, CompilationState state)
            {
                state.Write("redis");
                return true;
            }

            public bool VisitArrayTableDefinitionNode(ArrayTableDefinitionNode node, CompilationState state)
            {
                state.Write("{");
                WriteArguments(state, node.Elements);
                state.Write("}");
                return true;
            }

            public bool VisitDictionaryTableDefinition(DictionaryTableDefinitionNode node, CompilationState state)
            {
                state.Write("{");
                WriteArguments(state, node.Elements);
                state.Write("}");
                return true;
            }

            public bool VisitIteratorLoopNode(IteratorLoopNode node, CompilationState state)
            {
                if (node.Over.DataType == DataValueType.Array)
                {
                    state.Write("for _,");
                    state.Write(node.CursorName);
                    state.Write(" in ipairs(");
                    node.Over.AcceptVisitor(this, state);
                    state.Write(") do");
                    state.NewLine();
                    state.Ident();
                    node.Body.AcceptVisitor(this, state);
                    state.NewLine();
                    state.FinishIdent();
                    state.Write("end");
                }
                else if (node.Over.DataType == DataValueType.Dictionary)
                {
                    var keyTempIdent = CreateTemporaryIdentifier(state);
                    var valueTempIdent = CreateTemporaryIdentifier(state);
                    state.Write($"for {keyTempIdent},{valueTempIdent} in pairs(");
                    node.Over.AcceptVisitor(this, state);
                    state.Write(") do");
                    state.NewLine();
                    state.Ident();
                    state.Write($"local {node.CursorName} = ");
                    state.Write("{");
                    state.Write(keyTempIdent);
                    state.Write(",");
                    state.Write(valueTempIdent);
                    state.Write("};");
                    state.NewLine();
                    node.Body.AcceptVisitor(this, state);
                    state.NewLine();
                    state.FinishIdent();
                    state.Write("end");
                }
                else
                {
                    throw new LuaCompilationException($"Cannot iterate over '{node.Over.DataType}'");
                }

                return true;
            }

            private bool BooleanCasting(CompilationState state, ExpressionNode node)
            {
                // Check boolean according to original data type
                switch (node.DataType)
                {
                    case DataValueType.Float:
                        EncapsulateOrNot(state, node != (ConstantValueNode) 0);
                        break;
                    case DataValueType.Integer:
                        EncapsulateOrNot(state, node == (ConstantValueNode) 1);
                        break;
                    case DataValueType.String:
                        EncapsulateOrNot(state, node == (ConstantValueNode) "1");
                        break;
                    default:
                        node.AcceptVisitor(this, state);
                        break;
                }

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

            private void WriteArguments(CompilationState state,
                IEnumerable<KeyValuePair<ExpressionNode, ExpressionNode>> arguments, bool firstArgument = true)
            {
                WriteArguments<KeyValuePair<ExpressionNode, ExpressionNode>>(state, arguments, arg =>
                {
                    state.Write("[");
                    arg.Key.AcceptVisitor(this, state);
                    state.Write("]");
                    state.Write("=");
                    arg.Value.AcceptVisitor(this, state);
                }, firstArgument);
            }

            private void WriteArguments(CompilationState state, IList<ExpressionNode> arguments, bool firstArgument = true)
            {
                WriteArguments<ExpressionNode>(state, arguments, arg => { arg.AcceptVisitor(this, state); },
                    firstArgument);
            }

            private void WriteArguments<T>(CompilationState state, IEnumerable<T> arguments, Action<T> action,
                bool firstArgument = true)
            {
                bool first = true;
                foreach (var arg in arguments)
                {
                    if (!first || !firstArgument)
                    {
                        state.Write(", ");
                    }
                    action(arg);
                    first = false;
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
                    case BinaryExpressionOperator.NullCoalescing:
                        state.Write(" or ");
                        break;
                    case BinaryExpressionOperator.And:
                        state.Write(" and ");
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

            private void WriteString(CompilationState state, string str)
            {
                state.Write('"');
                state.Write(str);
                state.Write('"');
            }

            private string CreateTemporaryIdentifier(CompilationState state) => state.GetNewId();

            private string ResolveTemporaryIdentifier(CompilationState state, TemporaryIdentifierNode node) =>
                state.GetNewId(node.Id);
        }

        private RedILNode _root;

        private CompilationState _state;

        private IRedILVisitor<bool, CompilationState> _visitor;

        public CompilationInstance(RootNode root)
        {
            _root = root;
            _state = new CompilationState(root);
            _visitor = new Visitor();
        }

        public string Compile()
        {
            _root.AcceptVisitor(_visitor, _state);
            // Inserting functions on top
            foreach (var funcDef in _state.FunctionDefinitions)
            {
                _state.Builder.Insert(0, funcDef + Environment.NewLine);
            }
            return _state.Builder.ToString();
        }
    }
}