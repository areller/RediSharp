using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using RediSharp.CSharp;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Nodes.Internal;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Utilities;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace RediSharp.RedIL
{
    class CSharpCompiler
    {
        public MainResolver MainResolver { get; }
        
        class AstVisitor : IAstVisitor<RedILNode>
        {
            private CSharpCompiler _compiler;
            
            private DecompilationResult _csharp;

            private MainResolver _resolver;

            private HashSet<string> _identifiers;

            private RootNode _root;

            private Stack<BlockNode> _blockStack;

            public AstVisitor(CSharpCompiler compiler, DecompilationResult csharp)
            {
                _compiler = compiler;
                _csharp = csharp;
                _resolver = _compiler.MainResolver;
                _identifiers = new HashSet<string>();
                _identifiers.Add(csharp.ArgumentsVariableName);
                _identifiers.Add(csharp.CursorVariableName);
                _identifiers.Add(csharp.KeysVariableName);
                _blockStack = new Stack<BlockNode>();
            }

            /*
             * Not all of C#'s syntax tree is compiled
             * So the class is divided into `Used` and `Unused` sections
             */

            #region Used

            public RedILNode VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                return VisitArrayInitializerExpression(arrayCreateExpression.Initializer);
            }

            public RedILNode VisitAsExpression(AsExpression asExpression)
            {
                return asExpression.Expression.AcceptVisitor(this);
            }

            public RedILNode VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                if (assignmentExpression.Parent.NodeType != NodeType.Statement)
                {
                    throw new RedILException("Assigment is only possible within a statement");
                }

                var left = CastUtilities.CastRedILNode<ExpressionNode>(assignmentExpression.Left.AcceptVisitor(this));
                var right = CastUtilities.CastRedILNode<ExpressionNode>(assignmentExpression.Right.AcceptVisitor(this));

                if (assignmentExpression.Operator != AssignmentOperatorType.Assign)
                {
                    var op = OperatorUtilities.BinaryOperator(assignmentExpression.Operator);
                    right = CreateBinaryExpression(op, left, right);
                }

                return new AssignNode(left, right);
            }

            public RedILNode VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
            {
                var op = OperatorUtilities.BinaryOperator(binaryOperatorExpression.Operator);
                var left = CastUtilities.CastRedILNode<ExpressionNode>(
                    binaryOperatorExpression.Left.AcceptVisitor(this));
                var right = CastUtilities.CastRedILNode<ExpressionNode>(
                    binaryOperatorExpression.Right.AcceptVisitor(this));

                return CreateBinaryExpression(op, left, right);
            }

            public RedILNode VisitBlockStatement(BlockStatement blockStatement)
            {
                var block = new BlockNode();
                
                bool init = true;
                if (_root is null)
                {
                    init = false;
                    _root = new RootNode(block) {Identifiers = _identifiers};
                }

                /* No need to flatten implicit blocks for now */
                /*
                var children = blockStatement.Children
                    .SelectMany(child => FlattenImplicitBlocks(child.AcceptVisitor(this)))
                    .Where(child => child.Type != RedILNodeType.Empty);*/
                
                _blockStack.Push(block);
                foreach (var child in blockStatement.Children)
                {
                    var visited = child.AcceptVisitor(this);
                    if (visited.Type == RedILNodeType.Block)
                    {
                        foreach (var innerChild in ((BlockNode) visited).Children)
                        {
                            if (innerChild.Type != RedILNodeType.Empty)
                            {
                                block.Children.Add(innerChild);
                            }
                        }
                    }
                    else if (visited.Type != RedILNodeType.Empty)
                    {
                        block.Children.Add(visited);
                    }
                }
                _blockStack.Pop();

                if (!init)
                {
                    return _root;
                }

                return block;
            }

            public RedILNode VisitBreakStatement(BreakStatement breakStatement)
            {
                return new BreakNode();
            }

            public RedILNode VisitCastExpression(CastExpression castExpression)
            {
                var type = castExpression.Type as PrimitiveType;
                var resType = type is null ? DataValueType.Unknown : TypeUtilities.GetValueType(type.KnownTypeCode);

                var argument =
                    CastUtilities.CastRedILNode<ExpressionNode>(castExpression.Expression.AcceptVisitor(this));
                if (resType == DataValueType.Unknown || argument.Type == RedILNodeType.Nil)
                {
                    return argument;
                }

                return new CastNode(resType, argument);
            }

            public RedILNode VisitComment(Comment comment)
            {
                return new EmptyNode();
            }

            public RedILNode VisitConditionalExpression(ConditionalExpression conditionalExpression)
            {
                return new ConditionalExpressionNode(
                    CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.Condition.AcceptVisitor(this)),
                    CastUtilities.CastRedILNode<ExpressionNode>(
                        conditionalExpression.TrueExpression.AcceptVisitor(this)),
                    CastUtilities.CastRedILNode<ExpressionNode>(
                        conditionalExpression.FalseExpression.AcceptVisitor(this)));
            }

            public RedILNode VisitContinueStatement(ContinueStatement continueStatement)
            {
                return new ContinueNode();
            }

            public RedILNode VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression)
            {
                return new NilNode();
            }

            public RedILNode VisitDocumentationReference(DocumentationReference documentationReference)
            {
                return new EmptyNode();
            }

            public RedILNode VisitDoWhileStatement(DoWhileStatement doWhileStatement)
            {
                return new DoWhileNode(
                    CastUtilities.CastRedILNode<ExpressionNode>(doWhileStatement.Condition.AcceptVisitor(this)),
                    RemoveFirstLevelContinue(
                        CastUtilities.CastRedILNode<BlockNode>(doWhileStatement.EmbeddedStatement
                            .AcceptVisitor(this))));
            }

            public RedILNode VisitEmptyStatement(EmptyStatement emptyStatement)
            {
                return new EmptyNode();
            }

            public RedILNode VisitExpressionStatement(ExpressionStatement expressionStatement)
            {
                return expressionStatement.Expression.AcceptVisitor(this);
            }

            public RedILNode VisitForeachStatement(ForeachStatement foreachStatement)
            {
                var over = CastUtilities.CastRedILNode<ExpressionNode>(
                    foreachStatement.InExpression.AcceptVisitor(this));
                var body = CastUtilities.CastRedILNode<BlockNode>(
                    foreachStatement.EmbeddedStatement.AcceptVisitor(this));

                return new IteratorLoopNode(foreachStatement.VariableName, over, body);
            }

            public RedILNode VisitForStatement(ForStatement forStatement)
            {
                var blockNode = new BlockNode()
                {
                    Explicit = false
                };

                _blockStack.Push(blockNode);
                foreach (var initializer in forStatement.Initializers)
                {
                    var visited = initializer.AcceptVisitor(this);
                    blockNode.Children.Add(visited);
                }

                var whileNode = new WhileNode();
                whileNode.Condition =
                    CastUtilities.CastRedILNode<ExpressionNode>(forStatement.Condition.AcceptVisitor(this));
                whileNode.Body = RemoveFirstLevelContinue(CastUtilities.CastRedILNode<BlockNode>(
                    forStatement.EmbeddedStatement.AcceptVisitor(this)));

                foreach (var iterator in forStatement.Iterators)
                {
                    var visited = iterator.AcceptVisitor(this);
                    whileNode.Body.Children.Add(visited);
                }

                blockNode.Children.Add(whileNode);
                _blockStack.Pop();

                return blockNode;
            }

            public RedILNode VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                // Is Keys or Args
                if (identifierExpression.Identifier == _csharp.ArgumentsVariableName)
                {
                    return new ArgsTableNode();
                }
                else if (identifierExpression.Identifier == _csharp.KeysVariableName)
                {
                    return new KeysTableNode();
                }
                else if (identifierExpression.Identifier == _csharp.CursorVariableName)
                {
                    return new CursorNode();
                }

                var resType = _compiler.ResolveExpressionType(identifierExpression);
                return new IdentifierNode(identifierExpression.Identifier, resType);
            }

            public RedILNode VisitIfElseStatement(IfElseStatement ifElseStatement)
            {
                var ifNode = new IfNode();
                ifNode.Ifs = new[]
                {
                    new KeyValuePair<ExpressionNode, RedILNode>(
                        CastUtilities.CastRedILNode<ExpressionNode>(ifElseStatement.Condition.AcceptVisitor(this)),
                        NullIfNil(ifElseStatement.TrueStatement.AcceptVisitor(this)))
                }.Where(p => !(p.Value is null)).Where(p => !p.Key.EqualOrNull(ExpressionNode.False)).ToArray();
                var truth = ifNode.Ifs.FirstOrDefault(p => p.Key.EqualOrNull(ExpressionNode.True));
                if (!(truth.Key is null))
                {
                    return truth.Value;
                }
                
                ifNode.IfElse = NullIfNil(ifElseStatement.FalseStatement.AcceptVisitor(this));
                if (ifNode.Ifs.Count == 0 && ifNode.IfElse is null)
                {
                    return new EmptyNode();
                }
                else if (ifNode.Ifs.Count == 0)
                {
                    return ifNode.IfElse;
                }

                return ifNode;
            }

            public RedILNode VisitIndexerExpression(IndexerExpression indexerExpression)
            {
                var target = CastUtilities.CastRedILNode<ExpressionNode>(indexerExpression.Target.AcceptVisitor(this));
                var type = _compiler.ResolveExpressionType(indexerExpression);
                foreach (var arg in indexerExpression.Arguments)
                {
                    var argVisited = CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this));

                    // In LUA, array indices start at 1
                    if ((target.DataType == DataValueType.Array || target.DataType == DataValueType.String) &&
                        argVisited.DataType == DataValueType.Integer)
                    {
                        if (argVisited.Type == RedILNodeType.Constant)
                        {
                            argVisited = new ConstantValueNode(DataValueType.Integer,
                                int.Parse(((ConstantValueNode) argVisited).Value.ToString()) + 1);
                        }
                        else
                        {
                            argVisited = new BinaryExpressionNode(DataValueType.Integer, BinaryExpressionOperator.Add,
                                argVisited, new ConstantValueNode(DataValueType.Integer, 1));
                        }
                    }


                    if (target.DataType == DataValueType.Array || target.DataType == DataValueType.Dictionary)
                    {
                        target = new TableKeyAccessNode(target, argVisited, type);
                    }
                    else if (target.DataType == DataValueType.String)
                    {
                        target = new CallBuiltinLuaMethodNode(LuaBuiltinMethod.StringSub,
                            new List<ExpressionNode>() {target, argVisited, argVisited});
                    }
                }

                return target;
            }

            public RedILNode VisitInterpolatedStringExpression(
                InterpolatedStringExpression interpolatedStringExpression)
            {
                //TODO: set parent node
                var strings = new List<ExpressionNode>();
                foreach (var str in interpolatedStringExpression.Children)
                {
                    var child = CastUtilities.CastRedILNode<ExpressionNode>(str.AcceptVisitor(this));
                    strings.Add(child);
                }

                return new UniformOperatorNode(DataValueType.String, BinaryExpressionOperator.StringConcat, strings);
            }

            public RedILNode VisitInterpolatedStringText(InterpolatedStringText interpolatedStringText)
            {
                return new ConstantValueNode(DataValueType.String, interpolatedStringText.Text);
            }

            public RedILNode VisitInterpolation(Interpolation interpolation)
            {
                var expr = interpolation.Expression.AcceptVisitor(this);
                return expr;
            }

            public RedILNode VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                var memberReference = invocationExpression.Target as MemberReferenceExpression;
                if (memberReference is null)
                {
                    throw new RedILException($"Invocation is only possible by a member reference");
                }

                var isStatic = memberReference.Target is TypeReferenceExpression;
                
                var invocRes = _compiler.GetInvocationResolveResult(invocationExpression);
                var resolver = _resolver.ResolveMethod(isStatic, invocRes.DeclaringType,
                    memberReference.MemberName, invocRes.Parameters.ToArray());

                var caller = isStatic
                    ? null
                    : CastUtilities.CastRedILNode<ExpressionNode>(memberReference.Target.AcceptVisitor(this));

                var arguments = invocationExpression.Arguments
                    .Select(arg => CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this))).ToArray();

                return resolver.Resolve(GetContext(invocationExpression), caller, arguments);
            }

            public RedILNode VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                var target = memberReferenceExpression.Target;
                var isStatic = target is TypeReferenceExpression;
                /*
                var resolveResult =
                    memberReferenceExpression.Annotations.FirstOrDefault(annot => annot is MemberResolveResult) as
                        ResolveResult;

                if (resolveResult is null)
                {
                    resolveResult = target.Annotations.FirstOrDefault(annot => annot is ResolveResult) as ResolveResult;
                    if (resolveResult is null)
                    {
                        throw new RedILException("Unable to find member resolve annotation");
                    }
                }*/

                IType type;
                var resolveResult = target.Annotations.FirstOrDefault(annot => annot is ResolveResult) as ResolveResult;
                if (resolveResult is null)
                {
                    var memberResolveResult = memberReferenceExpression.Annotations.FirstOrDefault(annot => annot is MemberResolveResult) as
                        MemberResolveResult;
                    type = memberResolveResult.Member.DeclaringType;
                }
                else
                {
                    type = resolveResult.Type;
                }

                var resolver = _resolver.ResolveMember(isStatic, type,
                    memberReferenceExpression.MemberName);

                var caller = isStatic ? null : CastUtilities.CastRedILNode<ExpressionNode>(target.AcceptVisitor(this));

                return resolver.Resolve(GetContext(memberReferenceExpression), caller);
            }

            public RedILNode VisitNewLine(NewLineNode newLineNode)
            {
                return new EmptyNode();
            }

            public RedILNode VisitNullNode(AstNode nullNode)
            {
                return new NilNode();
            }

            public RedILNode VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression)
            {
                return new NilNode();
            }

            public RedILNode VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression)
            {
                return parenthesizedExpression.Expression.AcceptVisitor(this);
            }

            public RedILNode VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
            {
                var type = TypeUtilities.GetValueType(primitiveExpression.Value);
                return new ConstantValueNode(type, primitiveExpression.Value);
            }

            public RedILNode VisitReturnStatement(ReturnStatement returnStatement)
            {
                return new ReturnNode(
                    CastUtilities.CastRedILNode<ExpressionNode>(returnStatement.Expression.AcceptVisitor(this)));
            }

            public RedILNode VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
            {
                var operand =
                    CastUtilities.CastRedILNode<ExpressionNode>(unaryOperatorExpression.Expression.AcceptVisitor(this));
                if (OperatorUtilities.IsIncrement(unaryOperatorExpression.Operator))
                {
                    if (unaryOperatorExpression.Parent.NodeType != NodeType.Statement)
                    {
                        throw new RedILException($"Incremental operators can only be used within statements");
                    }

                    BinaryExpressionOperator binaryOp = default;
                    switch (unaryOperatorExpression.Operator)
                    {
                        case UnaryOperatorType.Increment:
                        case UnaryOperatorType.PostIncrement:
                            binaryOp = BinaryExpressionOperator.Add;
                            break;
                        case UnaryOperatorType.Decrement:
                        case UnaryOperatorType.PostDecrement:
                            binaryOp = BinaryExpressionOperator.Subtract;
                            break;
                    }

                    var constantOne = new ConstantValueNode(DataValueType.Integer, 1);
                    return new AssignNode(operand, CreateBinaryExpression(binaryOp, operand, constantOne));
                }

                var op = OperatorUtilities.UnaryOperator(unaryOperatorExpression.Operator);

                return new UnaryExpressionNode(op, operand);
            }

            public RedILNode VisitVariableDeclarationStatement(
                VariableDeclarationStatement variableDeclarationStatement)
            {
                var block = new BlockNode()
                {
                    Explicit = false
                };

                _blockStack.Push(block);
                foreach (var variable in variableDeclarationStatement.Variables)
                {
                    var decl = CastUtilities.CastRedILNode<VariableDeclareNode>(
                        variable.AcceptVisitor(this));
                    block.Children.Add(decl);
                }

                _blockStack.Pop();

                return block;
            }

            public RedILNode VisitVariableInitializer(VariableInitializer variableInitializer)
            {
                _identifiers.Add(variableInitializer.Name);
                return new VariableDeclareNode(variableInitializer.Name,
                    !(variableInitializer.Initializer is null)
                        ? CastUtilities.CastRedILNode<ExpressionNode>(
                            variableInitializer.Initializer.AcceptVisitor(this))
                        : null);
            }

            public RedILNode VisitWhileStatement(WhileStatement whileStatement)
            {
                var whileNode = new WhileNode();

                whileNode.Condition =
                    CastUtilities.CastRedILNode<ExpressionNode>(
                        whileStatement.Condition.AcceptVisitor(this));
                whileNode.Body = RemoveFirstLevelContinue(CastUtilities.CastRedILNode<BlockNode>(
                    whileStatement.EmbeddedStatement.AcceptVisitor(this)));

                return whileNode;
            }

            public RedILNode VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                return new EmptyNode();
            }
            
            public RedILNode VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                var invocRes = _compiler.GetInvocationResolveResult(objectCreateExpression);
                var resolver = _resolver.ResolveConstructor(invocRes.DeclaringType, invocRes.Parameters.ToArray());

                var args = objectCreateExpression.Arguments.Select(arg =>
                    CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this)));

                var initializerElements = objectCreateExpression.Initializer.Elements
                    .Select(elem => CastUtilities.CastRedILNode<ExpressionNode>(elem.AcceptVisitor(this))).ToArray();

                return resolver.Resolve(GetContext(objectCreateExpression), args.ToArray(), initializerElements);
            }
            
            public RedILNode VisitIsExpression(IsExpression isExpression)
            {
                var exprVisited = CastUtilities.CastRedILNode<ExpressionNode>(isExpression.Expression.AcceptVisitor(this));
                if (isExpression.Type.IsNull)
                {
                    return CreateBinaryExpression(BinaryExpressionOperator.Equal, exprVisited, ExpressionNode.Nil);
                }

                DataValueType type;
                if (isExpression.Type is PrimitiveType)
                {
                    type = TypeUtilities.GetValueType(((PrimitiveType) isExpression.Type).KnownTypeCode);
                }
                else
                {
                    type = DataValueType.Unknown;
                }

                if (type == DataValueType.Unknown)
                {
                    return ExpressionNode.False;
                }

                if (exprVisited.DataType == DataValueType.Unknown)
                {
                    return CreateBinaryExpression(BinaryExpressionOperator.Equal,
                        new CallBuiltinLuaMethodNode(LuaBuiltinMethod.Type, new[] {exprVisited}),
                        (ConstantValueNode) LuaTypeNameFromDataValueType(type));
                }

                return exprVisited.DataType == type ? ExpressionNode.True : ExpressionNode.False;
            }
            
            public RedILNode VisitSwitchStatement(SwitchStatement switchStatement)
            {
                BlockNode GetFromSection(SwitchSection section)
                {
                    if (section.Statements.Count != 1)
                    {
                        throw new RedILException("Expected statements inside switch section to be of length 1");
                    }

                    var block = CastUtilities.CastRedILNode<BlockNode>(section.Statements.First().AcceptVisitor(this));
                    var last = block.Children.LastOrDefault();
                    if (!(last is null) && last.Type == RedILNodeType.Break)
                    {
                        block.Children.Remove(last);
                    }

                    return block;
                }

                var pivot = CastUtilities.CastRedILNode<ExpressionNode>(switchStatement.Expression.AcceptVisitor(this));
                var ifNode = new IfNode();
                var defaultCase =
                    switchStatement.SwitchSections.FirstOrDefault(s =>
                        s.CaseLabels.Any(cl => cl.Expression is null || cl.Expression.IsNull));
                if (!(defaultCase is null))
                {
                    switchStatement.SwitchSections.Remove(defaultCase);
                }

                foreach (var section in switchStatement.SwitchSections)
                {
                    if (section.CaseLabels.Count == 0) continue;
                    var condition = CreateBinaryExpression(BinaryExpressionOperator.Equal, pivot,
                        CastUtilities.CastRedILNode<ExpressionNode>(section.CaseLabels.First().Expression
                            .AcceptVisitor(this)));
                    foreach (var or in section.CaseLabels.Skip(1))
                    {
                        condition = CreateBinaryExpression(BinaryExpressionOperator.Or, condition,
                            CreateBinaryExpression(BinaryExpressionOperator.Equal, pivot,
                                CastUtilities.CastRedILNode<ExpressionNode>(or.Expression.AcceptVisitor(this))));
                    }

                    if (condition.EqualOrNull(ExpressionNode.True))
                    {
                        return GetFromSection(section);
                    }
                    else if (!condition.EqualOrNull(ExpressionNode.False))
                    {
                        var block = GetFromSection(section);
                        ifNode.Ifs.Add(new KeyValuePair<ExpressionNode, RedILNode>(condition, block));
                    }
                }

                ifNode.IfElse = defaultCase is null ? null : GetFromSection(defaultCase);
                if (ifNode.Ifs.Count == 0 && ifNode.IfElse is null)
                {
                    return new EmptyNode();
                }
                else if (ifNode.Ifs.Count == 0)
                {
                    return ifNode.IfElse;
                }

                return ifNode;
            }

            #endregion

            #region Private

            private Context GetContext(Expression currentExpr)
            {
                return new Context(_compiler, _root, currentExpr, _blockStack.Peek());
            }
            
            private BinaryExpressionNode CreateBinaryExpression(BinaryExpressionOperator op, ExpressionNode left,
                ExpressionNode right)
            {
                if (OperatorUtilities.IsBoolean(op))
                {
                    return new BinaryExpressionNode(DataValueType.Boolean, op, left, right);
                }
                else if (OperatorUtilities.IsArithmatic(op))
                {
                    if (left.DataType == DataValueType.String ||
                        right.DataType == DataValueType.String)
                    {
                        return new BinaryExpressionNode(DataValueType.String, BinaryExpressionOperator.StringConcat,
                            left, right);
                    }
                    else if (left.DataType == DataValueType.Float ||
                             right.DataType == DataValueType.Float)
                    {
                        return new BinaryExpressionNode(DataValueType.Float, op, left, right);
                    }
                    else if (left.DataType == DataValueType.Integer &&
                             right.DataType == DataValueType.Integer)
                    {
                        return new BinaryExpressionNode(DataValueType.Integer, op, left, right);
                    }
                }
                else if (op == BinaryExpressionOperator.NullCoalescing)
                {
                    return new BinaryExpressionNode(left.DataType, op, left, right);
                }

                throw new RedILException(
                    $"Unsupported operator '{op}' with data types '{left.DataType}' and '{right.DataType}'");
            }

            //TODO: This covers the cases I've seen so far, might have to rewrite it to a more general version that would remove all instances of `continue`
            private BlockNode RemoveFirstLevelContinue(BlockNode node)
            {
                var newBlock = new BlockNode();
                for (int i = 0; i < node.Children.Count; i++)
                {
                    var child = node.Children[i];
                    if (child.Type == RedILNodeType.If)
                    {
                        var ifNode = child as IfNode;
                        if (!(ifNode.Ifs is null) && ifNode.Ifs.Count == 1 && ifNode.IfElse is null)
                        {
                            var truthBlock = ifNode.Ifs.First().Value as BlockNode;
                            if (truthBlock.Children.Count == 1 &&
                                truthBlock.Children.First().Type == RedILNodeType.Continue)
                            {
                                var innerIfBlock = new BlockNode();
                                for (int j = i + 1; j < node.Children.Count; j++)
                                {
                                    innerIfBlock.Children.Add(node.Children[j]);
                                }

                                /*newBlock.Children.Add(new IfNode(OptimizedNot(ifNode.Ifs.First().Key), innerIfBlock, null));*/
                                newBlock.Children.Add(new IfNode(
                                    new KeyValuePair<ExpressionNode, RedILNode>[]
                                    {
                                        new KeyValuePair<ExpressionNode, RedILNode>(
                                            OptimizedNot(ifNode.Ifs.First().Key), innerIfBlock)
                                    }, null));
                                break;
                            }
                        }
                    }

                    newBlock.Children.Add(child);
                }

                return newBlock;
            }

            private ExpressionNode OptimizedNot(ExpressionNode expr)
            {
                if (expr.Type == RedILNodeType.UnaryExpression &&
                    (expr as UnaryExpressionNode).Operator == UnaryExpressionOperator.Not)
                {
                    return (expr as UnaryExpressionNode).Operand;
                }

                return new UnaryExpressionNode(UnaryExpressionOperator.Not, expr);
            }

            private string LuaTypeNameFromDataValueType(DataValueType type)
            {
                switch (type)
                {
                    case DataValueType.Array:
                        return "table";
                    case DataValueType.Boolean:
                        return "boolean";
                    case DataValueType.Dictionary:
                        return "table";
                    case DataValueType.Float:
                        return "number";
                    case DataValueType.Integer:
                        return "number";
                    case DataValueType.String:
                        return "string";
                    case DataValueType.KVPair:
                        return "table";
                    default: return "nil";
                }
            }

            private RedILNode NullIfNil(RedILNode node)
            {
                return node.Type == RedILNodeType.Nil ||
                       (node.Type == RedILNodeType.Block &&
                        ((BlockNode) node).Children.SequenceEqual(new[] {ExpressionNode.Nil}))
                    ? null
                    : node;
            }

            private IEnumerable<RedILNode> FlattenImplicitBlocks(RedILNode node)
            {
                var stack = new Stack<RedILNode>();
                stack.Push(node);
                while (stack.Count > 0)
                {
                    var top = stack.Pop();
                    var topAsBlock = top as BlockNode;
                    if (topAsBlock is null || topAsBlock.Explicit)
                    {
                        yield return top;
                    }
                    else
                    {
                        var children = topAsBlock.Children.Reverse();
                        foreach (var child in children)
                        {
                            stack.Push(child);
                        }
                    }
                }
            }

            #endregion

            #region Unused

            public RedILNode VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitAnonymousTypeCreateExpression(
                AnonymousTypeCreateExpression anonymousTypeCreateExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitAccessor(Accessor accessor)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitConstructorInitializer(ConstructorInitializer constructorInitializer)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitEnumMemberDeclaration(EnumMemberDeclaration enumMemberDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitEventDeclaration(EventDeclaration eventDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitCustomEventDeclaration(CustomEventDeclaration customEventDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitMethodDeclaration(MethodDeclaration methodDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitParameterDeclaration(ParameterDeclaration parameterDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitFixedVariableInitializer(FixedVariableInitializer fixedVariableInitializer)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitSyntaxTree(SyntaxTree syntaxTree)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitSimpleType(SimpleType simpleType)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitMemberType(MemberType memberType)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitTupleType(TupleAstType tupleType)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitTupleTypeElement(TupleTypeElement tupleTypeElement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitComposedType(ComposedType composedType)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitArraySpecifier(ArraySpecifier arraySpecifier)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitUncheckedExpression(UncheckedExpression uncheckedExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryExpression(QueryExpression queryExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryContinuationClause(QueryContinuationClause queryContinuationClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryFromClause(QueryFromClause queryFromClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryLetClause(QueryLetClause queryLetClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryWhereClause(QueryWhereClause queryWhereClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryJoinClause(QueryJoinClause queryJoinClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryOrderClause(QueryOrderClause queryOrderClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryOrdering(QueryOrdering queryOrdering)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQuerySelectClause(QuerySelectClause querySelectClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitQueryGroupClause(QueryGroupClause queryGroupClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitAttribute(Attribute attribute)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitAttributeSection(AttributeSection attributeSection)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitTypeDeclaration(TypeDeclaration typeDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitUsingAliasDeclaration(UsingAliasDeclaration usingAliasDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitUsingDeclaration(UsingDeclaration usingDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitSwitchSection(SwitchSection switchSection)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitCaseLabel(CaseLabel caseLabel)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitThrowStatement(ThrowStatement throwStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitTryCatchStatement(TryCatchStatement tryCatchStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitCatchClause(CatchClause catchClause)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitUncheckedStatement(UncheckedStatement uncheckedStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitUnsafeStatement(UnsafeStatement unsafeStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitUsingStatement(UsingStatement usingStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitSizeOfExpression(SizeOfExpression sizeOfExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitStackAllocExpression(StackAllocExpression stackAllocExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitThrowExpression(ThrowExpression throwExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitTupleExpression(TupleExpression tupleExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitTypeOfExpression(TypeOfExpression typeOfExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitDirectionExpression(DirectionExpression directionExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitPreProcessorDirective(PreProcessorDirective preProcessorDirective)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitOutVarDeclarationExpression(OutVarDeclarationExpression outVarDeclarationExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitErrorNode(AstNode errorNode)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitPatternPlaceholder(AstNode placeholder, Pattern pattern)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitNamedExpression(NamedExpression namedExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitText(TextNode textNode)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitLambdaExpression(LambdaExpression lambdaExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitGotoStatement(GotoStatement gotoStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression)
            {
                var elements =
                    arrayInitializerExpression.Elements.Select(
                        elem => CastUtilities.CastRedILNode<ExpressionNode>(elem.AcceptVisitor(this)));
                
                return new ArrayTableDefinitionNode(elements.ToList());
            }

            public RedILNode VisitTypeParameterDeclaration(TypeParameterDeclaration typeParameterDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitConstraint(Constraint constraint)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitCSharpTokenNode(CSharpTokenNode cSharpTokenNode)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitIdentifier(Identifier identifier)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitFixedStatement(FixedStatement fixedStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitCheckedExpression(CheckedExpression checkedExpression)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitPrimitiveType(PrimitiveType primitiveType)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitLabelStatement(LabelStatement labelStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitLockStatement(LockStatement lockStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitCheckedStatement(CheckedStatement checkedStatement)
            {
                throw new NotImplementedException();
            }

            public RedILNode VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression)
            {
                throw new NotImplementedException();
            }

            #endregion
        }
        
        #region Public Utilities
        
        public DataValueType ResolveExpressionType(Expression expr)
            => ExtractTypeFromAnnontations(expr.Annotations);

        public DataValueType ExtractTypeFromAnnontations(IEnumerable<object> annontations)
        {
            var resType = DataValueType.Unknown;
            var ilResolveResult =
                annontations.FirstOrDefault(annot => annot is ResolveResult) as ResolveResult;

            if (!(ilResolveResult is null))
            {
                if (ilResolveResult.Type.Kind == TypeKind.Array)
                {
                    resType = DataValueType.Array;
                }
                else
                {
                    var systemType = Type.GetType(ilResolveResult.Type.ReflectionName);
                    resType = systemType is null ? DataValueType.Unknown : TypeUtilities.GetValueType(systemType);

                    if (resType == DataValueType.Unknown)
                    {
                        resType = MainResolver.ResolveDataType(ilResolveResult.Type);
                    }
                }
            }

            return resType;
        }

        public IParameterizedMember GetInvocationResolveResult(Expression expr)
        {
            var invocResult = expr.Annotations
                .FirstOrDefault(annot => annot is CSharpInvocationResolveResult) as CSharpInvocationResolveResult;

            if (invocResult is null)
            {
                throw new RedILException($"Unable to get invocation resolve from '{expr}'");
            }

            return invocResult.Member;
        }

        public RedILNode IfTable(Context context, DataValueType type, IList<KeyValuePair<ExpressionNode, ExpressionNode>> table)
        {
            table = table.Where(kv => !kv.Key.EqualOrNull(ExpressionNode.False)).ToList();
            var truth = table.SingleOrDefault(kv => kv.Key.EqualOrNull(ExpressionNode.True));
            if (!(truth.Key is null))
            {
                return truth.Value;
            }

            if (context.IsPartOfBlock())
            {
                var ifNode = new IfNode();
                ifNode.Ifs = table.Select(kv =>
                        new KeyValuePair<ExpressionNode, RedILNode>(kv.Key,
                            new BlockNode() {Children = new[] {kv.Value}}))
                    .ToList();
                return ifNode;
            }
            else
            {
                var temp = new TemporaryIdentifierNode(type);
                var ifNode = new IfNode();
                ifNode.Ifs = table.Select(kv => new KeyValuePair<ExpressionNode, RedILNode>(kv.Key,
                    new BlockNode() {Children = new[] {new AssignNode(temp, kv.Value)}})).ToList();
                
                context.CurrentBlock.Children.Add(new VariableDeclareNode(null, temp));
                context.CurrentBlock.Children.Add(ifNode);

                return temp;
            }
        }
        
        #endregion

        public CSharpCompiler()
        {
            MainResolver = new MainResolver();
        }

        public RootNode Compile(DecompilationResult csharp)
        {
            var visitor = new AstVisitor(this, csharp);
            var node = csharp.Body.AcceptVisitor(visitor);
            return node as RootNode;
        }
    }
}