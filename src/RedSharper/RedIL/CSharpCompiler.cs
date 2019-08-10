using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using RedSharper.CSharp;
using RedSharper.RedIL.Enums;
using RedSharper.RedIL.Utilities;
using System.Linq;
using ICSharpCode.Decompiler.CSharp;

namespace RedSharper.RedIL
{
    class CSharpCompiler
    {
        class State
        {
            public AstNode LastIterativeNode { get; set; }

            public AstNode ParentNode { get; set; }

            public State ParentState { get; set; }

            public State NewState(AstNode currentNode)
            {
                var lastIterative = LastIterativeNode;
                if (currentNode is DoWhileStatement ||
                    currentNode is WhileStatement ||
                    currentNode is ForStatement ||
                    currentNode is ForeachStatement)
                {
                    lastIterative = currentNode;
                }
                
                return new State()
                {
                    ParentState = this,
                    ParentNode = currentNode,
                    LastIterativeNode = lastIterative
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
                return asExpression.Expression.AcceptVisitor(this, data.NewState(asExpression));
            }
                
            public RedILNode VisitAssignmentExpression(AssignmentExpression assignmentExpression, State data)
            {
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
                var right = CastUtilities.CastRedILNode<ExpressionNode>(binaryOperatorExpression.AcceptVisitor(this, data.NewState(binaryOperatorExpression)));

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
                    .Select(child => child.AcceptVisitor(this, data))
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
                return castExpression.Expression.AcceptVisitor(this, data.NewState(castExpression));
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
                var condition = CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.AcceptVisitor(this, data.NewState(conditionalExpression)));
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
                var ilResolveResult = identifier.Annotations.Where(annot => annot is ILVariableResolveResult)
                    .FirstOrDefault() as ILVariableResolveResult;

                if (ilResolveResult == null)
                {
                    throw new RedILException($"Unable to find type annotation for identifier '{identifier.Name}'");
                }

                return null;
            }

            public RedILNode VisitIdentifierExpression(IdentifierExpression identifierExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitIfElseStatement(IfElseStatement ifElseStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitIndexerExpression(IndexerExpression indexerExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitInterpolatedStringExpression(InterpolatedStringExpression interpolatedStringExpression, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitInterpolatedStringText(InterpolatedStringText interpolatedStringText, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitInterpolation(Interpolation interpolation, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitInvocationExpression(InvocationExpression invocationExpression, State data)
            {
                throw new System.NotImplementedException();
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
                throw new System.NotImplementedException();
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
                throw new System.NotImplementedException();
            }

            public RedILNode VisitNullNode(AstNode nullNode, State data)
            {
                throw new System.NotImplementedException();
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
                throw new System.NotImplementedException();
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
                throw new System.NotImplementedException();
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
                throw new System.NotImplementedException();
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
                throw new System.NotImplementedException();
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
                throw new System.NotImplementedException();
            }

            public RedILNode VisitVariableInitializer(VariableInitializer variableInitializer, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitWhileStatement(WhileStatement whileStatement, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitWhitespace(WhitespaceNode whitespaceNode, State data)
            {
                throw new System.NotImplementedException();
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