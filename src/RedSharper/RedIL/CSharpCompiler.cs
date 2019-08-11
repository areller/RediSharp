using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using RedSharper.CSharp;
using RedSharper.RedIL.Enums;
using RedSharper.RedIL.Utilities;
using System.Linq;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using RedSharper.Contracts;
using RedSharper.Enums;
using RedSharper.Extensions;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace RedSharper.RedIL
{
    class CSharpCompiler
    {
        class State
        {
            public AstNode LastIterativeNode { get; set; }

            public SwitchStatement LastSwitchStatement { get; set; }

            public AstNode ParentNode { get; set; }

            public State ParentState { get; set; }

            public State NewState(AstNode currentNode)
            {
                var lastIterative = LastIterativeNode;
                var lastSwitchStmt = LastSwitchStatement;
                
                if (currentNode is DoWhileStatement ||
                    currentNode is WhileStatement ||
                    currentNode is ForStatement ||
                    currentNode is ForeachStatement)
                {
                    lastIterative = currentNode;
                }
                else if (currentNode is SwitchStatement)
                {
                    lastSwitchStmt = currentNode as SwitchStatement;
                }
                
                return new State()
                {
                    ParentState = this,
                    ParentNode = currentNode,
                    LastIterativeNode = lastIterative,
                    LastSwitchStatement = lastSwitchStmt
                };
            }
        }

        class AstVisitor : IAstVisitor<State, RedILNode>
        {
            private DecompilationResult _csharp;

            public AstVisitor(DecompilationResult csharp)
            {
                _csharp = csharp;
            }

            public RedILNode VisitAccessor(Accessor accessor, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitAnonymousTypeCreateExpression(AnonymousTypeCreateExpression anonymousTypeCreateExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression, State data)
            {
                //TODO: Create Table of Constant Size
                throw new System.NotImplementedException();
            }

            public RedILNode VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression, State data)
            {
                //TODO: Create table of constant size with data
                throw new System.NotImplementedException();
            }

            public RedILNode VisitArraySpecifier(ArraySpecifier arraySpecifier, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitAsExpression(AsExpression asExpression, State data)
            {
                // We only support casting into primitives for now
                return asExpression.Expression.AcceptVisitor(this, data.NewState(asExpression));
            }
                
            public RedILNode VisitAssignmentExpression(AssignmentExpression assignmentExpression, State data)
            {
                if (data.ParentNode.NodeType != NodeType.Statement)
                {
                    throw new RedILException("Assigment is only possible within a statement");
                }
                
                var left = CastUtilities.CastRedILNode<ExpressionNode>(assignmentExpression.Left.AcceptVisitor(this, data.NewState(assignmentExpression)));
                var right = CastUtilities.CastRedILNode<ExpressionNode>(assignmentExpression.Right.AcceptVisitor(this, data.NewState(assignmentExpression)));

                if (assignmentExpression.Operator == AssignmentOperatorType.Assign)
                {
                    return new AssignNode(left, right);
                }
                else
                {
                    var op = OperatorUtilities.BinaryOperator(assignmentExpression.Operator);
                    return new AssignNode(left, VisitBinaryOperatorExpression(left, right, op, data));
                }
            }

            public RedILNode VisitAttribute(Attribute attribute, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitAttributeSection(AttributeSection attributeSection, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression, State data)
            {
                var op = OperatorUtilities.BinaryOperator(binaryOperatorExpression.Operator);
                var left = CastUtilities.CastRedILNode<ExpressionNode>(binaryOperatorExpression.Left.AcceptVisitor(this, data.NewState(binaryOperatorExpression)));
                var right = CastUtilities.CastRedILNode<ExpressionNode>(binaryOperatorExpression.Right.AcceptVisitor(this, data.NewState(binaryOperatorExpression)));

                return VisitBinaryOperatorExpression(left, right, op, data);
            }

            private ExpressionNode VisitBinaryOperatorExpression(ExpressionNode left, ExpressionNode right, BinaryExpressionOperator op, State data)
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
                        return new BinaryExpressionNode(DataValueType.String, BinaryExpressionOperator.StringConcat, left, right);
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

                throw new RedILException($"Unsupported operator '{op}' with data types '{left.DataType}' and '{right.DataType}'");
            }

            public RedILNode VisitBlockStatement(BlockStatement blockStatement, State data)
            {
                return new BlockNode(blockStatement.Children
                    .Select(child => child.AcceptVisitor(this, data.NewState(blockStatement)))
                    .Where(child => child.Type != RedILNodeType.Empty)
                    .ToList());
            }

            public RedILNode VisitBreakStatement(BreakStatement breakStatement, State data)
            {
                return new BreakNode();
            }

            public RedILNode VisitCaseLabel(CaseLabel caseLabel, State data)
            {
                //TODO: Handle switch/case
                throw new System.NotImplementedException();
            }

            public RedILNode VisitCastExpression(CastExpression castExpression, State data)
            {
                var type = castExpression.Type as PrimitiveType;
                if (type == null)
                {
                    throw new RedILException($"Only supports casting to primitive types");
                }

                var resType = TypeUtilities.GetValueType(type.KnownTypeCode);
                var expr = CastUtilities.CastRedILNode<ExpressionNode>(castExpression.Expression.AcceptVisitor(this, data.NewState(castExpression)));

                return new CastNode(resType, expr);
            }

            public RedILNode VisitCatchClause(CatchClause catchClause, State data)
            {
                //TOOD: Check if try/catch needed
                throw new System.NotImplementedException();
            }

            public RedILNode VisitCheckedExpression(CheckedExpression checkedExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitCheckedStatement(CheckedStatement checkedStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitComment(Comment comment, State data)
            {
                return new EmptyNode();
            }

            public RedILNode VisitComposedType(ComposedType composedType, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitConditionalExpression(ConditionalExpression conditionalExpression, State data)
            {
                var condition = CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.Condition.AcceptVisitor(this, data.NewState(conditionalExpression)));
                var ifYes = CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.TrueExpression.AcceptVisitor(this, data.NewState(conditionalExpression)));
                var ifNo = CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.FalseExpression.AcceptVisitor(this, data.NewState(conditionalExpression)));

                return new ConditionalExpressionNode(condition, ifYes, ifNo);
            }

            public RedILNode VisitConstraint(Constraint constraint, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitConstructorInitializer(ConstructorInitializer constructorInitializer, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitContinueStatement(ContinueStatement continueStatement, State data)
            {
                //TODO: Handle continue with labels and goto
                var lastIterative = data.LastIterativeNode;
                throw new System.NotImplementedException();
            }

            public RedILNode VisitCSharpTokenNode(CSharpTokenNode cSharpTokenNode, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitCustomEventDeclaration(CustomEventDeclaration customEventDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression, State data)
            {
                return new NilNode();
            }

            public RedILNode VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitDirectionExpression(DirectionExpression directionExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitDocumentationReference(DocumentationReference documentationReference, State data)
            {
                return new EmptyNode();
            }

            public RedILNode VisitDoWhileStatement(DoWhileStatement doWhileStatement, State data)
            {
                var condition = CastUtilities.CastRedILNode<ExpressionNode>(doWhileStatement.Condition.AcceptVisitor(this, data.NewState(doWhileStatement)));
                var body = CastUtilities.CastRedILNode<BlockNode>(doWhileStatement.EmbeddedStatement.AcceptVisitor(this, data.NewState(doWhileStatement)));
                
                return new DoWhileNode(condition, body);
            }

            public RedILNode VisitEmptyStatement(EmptyStatement emptyStatement, State data)
            {
                return new EmptyNode();
            }

            public RedILNode VisitEnumMemberDeclaration(EnumMemberDeclaration enumMemberDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitErrorNode(AstNode errorNode, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitEventDeclaration(EventDeclaration eventDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitExpressionStatement(ExpressionStatement expressionStatement, State data)
            {
                return expressionStatement.Expression.AcceptVisitor(this, data.NewState(expressionStatement));
            }

            public RedILNode VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitFieldDeclaration(FieldDeclaration fieldDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitFixedStatement(FixedStatement fixedStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitFixedVariableInitializer(FixedVariableInitializer fixedVariableInitializer, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitForeachStatement(ForeachStatement foreachStatement, State data)
            {
                //TODO: Handle foreach
                throw new System.NotImplementedException();
            }

            public RedILNode VisitForStatement(ForStatement forStatement, State data)
            {
                var blockNode = new BlockNode()
                {
                    Explicit = false
                };

                foreach (var initializer in forStatement.Initializers)
                {
                    var visited = initializer.AcceptVisitor(this, data.NewState(forStatement));
                    blockNode.Children.Add(visited);
                }

                var condition = CastUtilities.CastRedILNode<ExpressionNode>(forStatement.Condition.AcceptVisitor(this, data.NewState(forStatement)));
                var body = CastUtilities.CastRedILNode<BlockNode>(forStatement.EmbeddedStatement.AcceptVisitor(this, data.NewState(forStatement)));

                foreach (var iterator in forStatement.Iterators)
                {
                    var visited = iterator.AcceptVisitor(this, data.NewState(forStatement));
                    body.Children.Add(visited);
                }
                
                var whileNode = new WhileNode(condition, body);
                blockNode.Children.Add(whileNode);

                return blockNode;
            }

            public RedILNode VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitGotoStatement(GotoStatement gotoStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitIdentifier(Identifier identifier, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitIdentifierExpression(IdentifierExpression identifierExpression, State data)
            {
                //TOOD: What is the difference between this and identifier?
                var resType = DataValueType.Unknown;
                var ilResolveResult = identifierExpression.Annotations.Where(annot => annot is ILVariableResolveResult)
                    .FirstOrDefault() as ILVariableResolveResult;

                if (ilResolveResult != null)
                {
                    if (ilResolveResult.Type.Kind != TypeKind.Array)
                    {
                        var type = Type.GetType(ilResolveResult.Type.FullName);
                        resType = TypeUtilities.GetValueType(type);
                    }
                    else
                    {
                        resType = DataValueType.Multi;
                    }
                }
                
                return new IdentifierNode(identifierExpression.Identifier, resType);
            }

            public RedILNode VisitIfElseStatement(IfElseStatement ifElseStatement, State data)
            {
                var condition = ifElseStatement.Condition.AcceptVisitor(this, data.NewState(ifElseStatement));
                var ifTrue = ifElseStatement.TrueStatement.AcceptVisitor(this, data.NewState(ifElseStatement));
                var ifFalse = ifElseStatement.FalseStatement.AcceptVisitor(this, data.NewState(ifElseStatement));
                
                return new IfNode(condition, ifTrue, ifFalse);
            }

            public RedILNode VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitIndexerExpression(IndexerExpression indexerExpression, State data)
            {
                var target = CastUtilities.CastRedILNode<ExpressionNode>(indexerExpression.Target.AcceptVisitor(this, data.NewState(indexerExpression)));
                foreach (var arg in indexerExpression.Arguments)
                {
                    var argVisited = CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this, data.NewState(indexerExpression)));
                    target = new TableKeyAccessNode(target, argVisited);
                }

                return target;
            }

            public RedILNode VisitInterpolatedStringExpression(InterpolatedStringExpression interpolatedStringExpression, State data)
            {
                var strings = new List<ExpressionNode>();
                foreach (var str in interpolatedStringExpression.Children)
                {
                    var child = CastUtilities.CastRedILNode<ExpressionNode>(str.AcceptVisitor(this, data.NewState(interpolatedStringExpression)));
                    strings.Add(child);
                }
                
                return new UniformOperatorNode(DataValueType.String, BinaryExpressionOperator.StringConcat, strings);
            }

            public RedILNode VisitInterpolatedStringText(InterpolatedStringText interpolatedStringText, State data)
            {
                return new ConstantValueNode(DataValueType.String, interpolatedStringText.Text);
            }

            public RedILNode VisitInterpolation(Interpolation interpolation, State data)
            {
                var expr = interpolation.Expression.AcceptVisitor(this, data.NewState(interpolation));
                return expr;
            }

            public RedILNode VisitInvocationExpression(InvocationExpression invocationExpression, State data)
            {
                var memberReference = invocationExpression.Target as MemberReferenceExpression;
                if (memberReference == null)
                {
                    throw new RedILException("Invocation can only be made from a member reference");
                }

                var targetType = memberReference.Target as TypeReferenceExpression;
                if (targetType == null)
                {
                    throw new RedILException("Invocation can only be made from a static type reference");
                }

                var args = new List<ExpressionNode>();
                foreach (var arg in invocationExpression.Arguments)
                {
                    var argVisited = CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this, data.NewState(invocationExpression)));
                    args.Add(argVisited);
                }

                if (targetType.Type is PrimitiveType)
                {
                    var primitiveType = targetType.Type as PrimitiveType;
                    switch ($"{primitiveType.Keyword}.{memberReference.MemberName}")
                    {
                        case "int.Parse":
                        case "long.Parse":
                            return new CastNode(DataValueType.Integer, args.First());
                        case "double.Parse":
                        case "decimal.Parse":
                        case "float.Parse":
                            return new CastNode(DataValueType.Float, args.First());
                        case "bool.Parse":
                            return new BinaryExpressionNode(
                                DataValueType.Boolean,
                                BinaryExpressionOperator.Equal,
                                new CallLuaMethodNode(LuaMethod.StringToLower, new ExpressionNode[] {args.First()}),
                                new ConstantValueNode(DataValueType.String, "true"));
                        default:
                            throw new RedILException(
                                $"Invalid primitive type invocation '{primitiveType.Keyword}.{memberReference.MemberName}'");
                    }
                }
                else if (targetType.Type is SimpleType)
                {
                    var simpleType = targetType.Type as SimpleType;
                    var cmdArgs = args.Skip(1).ToArray();
                    
                    switch ($"{simpleType.Identifier}.{memberReference.MemberName}")
                    {
                        case "CursorExtensions.Get":
                            return new CallRedisMethodNode(RedisCommand.Get, cmdArgs);
                        case "CursorExtensions.Set":
                            return new CallRedisMethodNode(RedisCommand.Set, cmdArgs);
                        case "CursorExtensions.HGet":
                            return new CallRedisMethodNode(RedisCommand.HGet, cmdArgs);
                        case "CursorExtensions.HMGet":
                            return new CallRedisMethodNode(RedisCommand.HMGet, cmdArgs);
                        case "CursorExtensions.HSet":
                            return new CallRedisMethodNode(RedisCommand.Set, cmdArgs);
                        default:
                            throw new RedILException($"Unsupported Redis method '{memberReference.MemberName}'");
                    }
                }
                else
                {
                    throw new RedILException($"Invalid type '{targetType.Type.GetType().Name}' for invocation");
                }
            }

            public RedILNode VisitIsExpression(IsExpression isExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitLabelStatement(LabelStatement labelStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitLambdaExpression(LambdaExpression lambdaExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitLockStatement(LockStatement lockStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression, State data)
            {
                //Note: this handles explicit member references, invocations are handled solly by VisitInvocationExpression
                var typeName = ((memberReferenceExpression.Target as TypeReferenceExpression)?.Type as SimpleType)
                    ?.Identifier;
                if (typeName == null)
                {
                    throw new RedILException($"Only static classes members are supported");
                }

                switch (typeName)
                {
                    case nameof(RedResult):
                    {
                        switch (memberReferenceExpression.MemberName)
                        {
                            case nameof(RedResult.Ok):
                                return new StatusNode(Status.Ok);
                            default: throw new RedILException($"Unsupported member '{memberReferenceExpression.MemberName}' in '{typeName}'");
                        }
                        break;
                    }
                    default: throw new RedILException($"Unsupported static type '{typeName}'");
                }
            }

            public RedILNode VisitMemberType(MemberType memberType, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitMethodDeclaration(MethodDeclaration methodDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression, State data)
            {
                //TODO: Check if needed
                throw new System.NotImplementedException();
            }

            public RedILNode VisitNamedExpression(NamedExpression namedExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitNewLine(NewLineNode newLineNode, State data)
            {
                return new EmptyNode();
            }

            public RedILNode VisitNullNode(AstNode nullNode, State data)
            {
                return new NilNode();
            }

            public RedILNode VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitOutVarDeclarationExpression(OutVarDeclarationExpression outVarDeclarationExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitParameterDeclaration(ParameterDeclaration parameterDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression, State data)
            {
                return parenthesizedExpression.Expression.AcceptVisitor(this, data.NewState(parenthesizedExpression));
            }

            public RedILNode VisitPatternPlaceholder(AstNode placeholder, Pattern pattern, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitPreProcessorDirective(PreProcessorDirective preProcessorDirective, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitPrimitiveExpression(PrimitiveExpression primitiveExpression, State data)
            {
                var type = TypeUtilities.GetValueType(primitiveExpression.Value);
                return new ConstantValueNode(type, primitiveExpression.Value);
            }

            public RedILNode VisitPrimitiveType(PrimitiveType primitiveType, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryContinuationClause(QueryContinuationClause queryContinuationClause, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryExpression(QueryExpression queryExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryFromClause(QueryFromClause queryFromClause, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryGroupClause(QueryGroupClause queryGroupClause, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryJoinClause(QueryJoinClause queryJoinClause, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryLetClause(QueryLetClause queryLetClause, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryOrderClause(QueryOrderClause queryOrderClause, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryOrdering(QueryOrdering queryOrdering, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQuerySelectClause(QuerySelectClause querySelectClause, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitQueryWhereClause(QueryWhereClause queryWhereClause, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitReturnStatement(ReturnStatement returnStatement, State data)
            {
                var returned = CastUtilities.CastRedILNode<ExpressionNode>(returnStatement.Expression.AcceptVisitor(this, data.NewState(returnStatement)));
                return new ReturnNode(returned);
            }

            public RedILNode VisitSimpleType(SimpleType simpleType, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitSizeOfExpression(SizeOfExpression sizeOfExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitStackAllocExpression(StackAllocExpression stackAllocExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitSwitchSection(SwitchSection switchSection, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitSwitchStatement(SwitchStatement switchStatement, State data)
            {
                //TODO: Handle switch
                throw new System.NotImplementedException();
            }

            public RedILNode VisitSyntaxTree(SyntaxTree syntaxTree, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitText(TextNode textNode, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitThrowExpression(ThrowExpression throwExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitThrowStatement(ThrowStatement throwStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitTryCatchStatement(TryCatchStatement tryCatchStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitTupleExpression(TupleExpression tupleExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitTupleType(TupleAstType tupleType, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitTupleTypeElement(TupleTypeElement tupleTypeElement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitTypeDeclaration(TypeDeclaration typeDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitTypeOfExpression(TypeOfExpression typeOfExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitTypeParameterDeclaration(TypeParameterDeclaration typeParameterDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression, State data)
            {
                var operand = CastUtilities.CastRedILNode<ExpressionNode>(unaryOperatorExpression.Expression.AcceptVisitor(this, data.NewState(unaryOperatorExpression)));
                if (OperatorUtilities.IsIncrement(unaryOperatorExpression.Operator))
                {
                    if (data.ParentNode.NodeType != NodeType.Statement)
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
                    return new AssignNode(operand, VisitBinaryOperatorExpression(operand, constantOne, binaryOp, data.NewState(unaryOperatorExpression)));
                }

                var op = OperatorUtilities.UnaryOperator(unaryOperatorExpression.Operator);
                
                return new UnaryExpressionNode(op, operand);
            }

            public RedILNode VisitUncheckedExpression(UncheckedExpression uncheckedExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitUncheckedStatement(UncheckedStatement uncheckedStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitUnsafeStatement(UnsafeStatement unsafeStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitUsingAliasDeclaration(UsingAliasDeclaration usingAliasDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitUsingDeclaration(UsingDeclaration usingDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitUsingStatement(UsingStatement usingStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement, State data)
            {
                var block = new BlockNode()
                {
                    Explicit = false
                };

                foreach (var variable in variableDeclarationStatement.Variables)
                {
                    var decl = CastUtilities.CastRedILNode<VariableDeclareNode>(
                        variable.AcceptVisitor(this, data.NewState(variableDeclarationStatement)));
                    block.Children.Add(decl);
                }

                return block;
            }

            public RedILNode VisitVariableInitializer(VariableInitializer variableInitializer, State data)
            {
                var expr = variableInitializer.Initializer != null
                    ? CastUtilities.CastRedILNode<ExpressionNode>(
                        variableInitializer.Initializer.AcceptVisitor(this, data.NewState(variableInitializer)))
                    : null;
                return new VariableDeclareNode(variableInitializer.Name, expr);
            }

            public RedILNode VisitWhileStatement(WhileStatement whileStatement, State data)
            {
                var condition =
                    CastUtilities.CastRedILNode<ExpressionNode>(
                        whileStatement.Condition.AcceptVisitor(this, data.NewState(whileStatement)));
                var body = CastUtilities.CastRedILNode<BlockNode>(
                    whileStatement.EmbeddedStatement.AcceptVisitor(this, data.NewState(whileStatement)));
                
                return new WhileNode(condition, body);
            }

            public RedILNode VisitWhitespace(WhitespaceNode whitespaceNode, State data)
            {
                return new EmptyNode();
            }

            public RedILNode VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement, State data)
            {
                throw new System.NotImplementedException();
            }
        }

        public RedILNode Compile(DecompilationResult csharp)
            => csharp.Body.AcceptVisitor(new AstVisitor(csharp), new State());
    }
}