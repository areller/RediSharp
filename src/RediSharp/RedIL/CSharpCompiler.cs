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
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Nodes.Internal;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Utilities;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace RediSharp.RedIL
{
    class CSharpCompiler
    {
        private MainResolver _mainResolver;
        
        class AstVisitor : IAstVisitor<RedILNode>
        {
            private CSharpCompiler _compiler;

            private DecompilationResult _csharp;

            private MainResolver _resolver;

            public AstVisitor(CSharpCompiler compiler, DecompilationResult csharp)
            {
                _compiler = compiler;
                _csharp = csharp;
                _resolver = _compiler._mainResolver;
            }

            /*
             * Not all of C#'s syntax tree is compiled
             * So the class is divided into `Used` and `Unused` sections
             */

            #region Used

            public RedILNode VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                return new ArrayTableDefinitionNode(arrayCreateExpression.Initializer.Elements.Select(elem =>
                    CastUtilities.CastRedILNode<ExpressionNode>(elem.AcceptVisitor(this))).ToList());
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
                return new BlockNode(blockStatement.Children.Select(child => child.AcceptVisitor(this))
                    .Where(child => child.Type != RedILNodeType.Empty).ToList());
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
                if (argument.Type == RedILNodeType.Nil)
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
                //TODO: Find how to handle for each loops over dictionaries
                // In case of for each over arrays, there is a 1=>1 map between the Lua variable and the C# variable,
                // In for each over dictionary, one C# variable (KeyValuePair) maps to 2 Lua variables
                var cursorType = ExtractTypeFromAnnontations(foreachStatement.Annotations);
                return new IteratorLoopNode(cursorType, foreachStatement.VariableName,
                    CastUtilities.CastRedILNode<ExpressionNode>(foreachStatement.InExpression.AcceptVisitor(this)),
                    CastUtilities.CastRedILNode<BlockNode>(foreachStatement.EmbeddedStatement.AcceptVisitor(this)));
            }

            public RedILNode VisitForStatement(ForStatement forStatement)
            {
                var blockNode = new BlockNode()
                {
                    Explicit = false
                };

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

                var resType = ResolveExpressionType(identifierExpression);
                return new IdentifierNode(identifierExpression.Identifier, resType);
            }

            public RedILNode VisitIfElseStatement(IfElseStatement ifElseStatement)
            {
                var ifNode = new IfNode();
                ifNode.Condition =
                    CastUtilities.CastRedILNode<ExpressionNode>(ifElseStatement.Condition.AcceptVisitor(this));
                ifNode.IfTrue = ifElseStatement.TrueStatement.AcceptVisitor(this);
                ifNode.IfFalse = ifElseStatement.FalseStatement.AcceptVisitor(this);

                if (ifNode.IfTrue is NilNode) ifNode.IfTrue = null;
                if (ifNode.IfFalse is NilNode) ifNode.IfFalse = null;

                return ifNode;
            }

            public RedILNode VisitIndexerExpression(IndexerExpression indexerExpression)
            {
                var target = CastUtilities.CastRedILNode<ExpressionNode>(indexerExpression.Target.AcceptVisitor(this));
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
                        target = new TableKeyAccessNode(target, argVisited);
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
                
                var invocRes = GetInvocationResolveResult(invocationExpression);
                var resolver = _resolver.ResolveMethod(isStatic, invocRes.DeclaringType,
                    memberReference.MemberName, invocRes.Parameters.ToArray());

                var caller = isStatic
                    ? null
                    : CastUtilities.CastRedILNode<ExpressionNode>(memberReference.Target.AcceptVisitor(this));

                var arguments = invocationExpression.Arguments
                    .Select(arg => CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this))).ToArray();

                return resolver.Resolve(GetContext(), caller, arguments);
            }

            public RedILNode VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                var isStatic = memberReferenceExpression.Target is TypeReferenceExpression;
                var resolveResult =
                    memberReferenceExpression.Annotations.FirstOrDefault(annot => annot is MemberResolveResult) as
                        MemberResolveResult;

                if (resolveResult is null)
                {
                    throw new RedILException("Unable to find member resolve annotation");
                }

                var resolver = _resolver.ResolveMember(isStatic, resolveResult.Member.DeclaringType,
                    memberReferenceExpression.MemberName);

                var caller = isStatic ? null : CastUtilities.CastRedILNode<ExpressionNode>(memberReferenceExpression.Target.AcceptVisitor(this));

                return resolver.Resolve(GetContext(), caller);
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
                    if (unaryOperatorExpression.NodeType != NodeType.Statement)
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

                foreach (var variable in variableDeclarationStatement.Variables)
                {
                    var decl = CastUtilities.CastRedILNode<VariableDeclareNode>(
                        variable.AcceptVisitor(this));
                    block.Children.Add(decl);
                }

                return block;
            }

            public RedILNode VisitVariableInitializer(VariableInitializer variableInitializer)
            {
                return new VariableDeclareNode(variableInitializer.Name,
                    variableInitializer.Initializer != null
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
                var invocRes = GetInvocationResolveResult(objectCreateExpression);
                var resolver = _resolver.ResolveConstructor(invocRes.DeclaringType, invocRes.Parameters.ToArray());

                var args = objectCreateExpression.Arguments.Select(arg =>
                    CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this)));

                var initializerElements = objectCreateExpression.Initializer.Elements
                    .Select(elem => CastUtilities.CastRedILNode<ExpressionNode>(elem.AcceptVisitor(this))).ToArray();

                return resolver.Resolve(GetContext(), args.ToArray(), initializerElements);
            }

            #endregion

            #region Private

            private Context GetContext()
            {
                return null;
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
                        if (ifNode.IfTrue != null && ifNode.IfFalse == null)
                        {
                            var truthBlock = ifNode.IfTrue as BlockNode;
                            if (truthBlock.Children.Count == 1 &&
                                truthBlock.Children.First().Type == RedILNodeType.Continue)
                            {
                                var innerIfBlock = new BlockNode();
                                for (int j = i + 1; j < node.Children.Count; j++)
                                {
                                    innerIfBlock.Children.Add(node.Children[j]);
                                }

                                newBlock.Children.Add(new IfNode(OptimizedNot(ifNode.Condition), innerIfBlock, null));
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

            #region Types
            
            private DataValueType ResolveExpressionType(Expression expr)
                => ExtractTypeFromAnnontations(expr.Annotations);

            private DataValueType ExtractTypeFromAnnontations(IEnumerable<object> annontations)
            {
                var resType = DataValueType.Unknown;
                var ilResolveResult =
                    annontations.FirstOrDefault(annot => annot is ILVariableResolveResult) as ILVariableResolveResult;

                if (ilResolveResult != null)
                {
                    if (ilResolveResult.Type.Kind != TypeKind.Array)
                    {
                        var type = Type.GetType(ilResolveResult.Type.ReflectionName);
                        resType = TypeUtilities.GetValueType(type);
                    }
                    else
                    {
                        //TODO: Handle list/dictionary types
                        resType = DataValueType.Array;
                    }
                }

                return resType;
            }

            private IParameterizedMember GetInvocationResolveResult(Expression expr)
            {
                var invocResult = expr.Annotations
                    .FirstOrDefault(annot => annot is CSharpInvocationResolveResult) as CSharpInvocationResolveResult;

                if (invocResult is null)
                {
                    throw new RedILException($"Unable to get invocation resolve from '{expr}'");
                }

                return invocResult.Member;
            }

            #endregion

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

            public RedILNode VisitSwitchStatement(SwitchStatement switchStatement)
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

            public RedILNode VisitIsExpression(IsExpression isExpression)
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
                throw new NotImplementedException();
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

        public CSharpCompiler()
        {
            _mainResolver = new MainResolver();
        }

        public RedILNode Compile(DecompilationResult csharp)
        {
            var node = csharp.Body.AcceptVisitor(new AstVisitor(this, csharp));
            return new RootNode(node);
        }
    }
}