using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using System.Linq;
using System.Reflection;
using System.Threading;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using RediSharp.CSharp;
using RediSharp.RedIL.Attributes;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.ExternalResolvers;
using RediSharp.RedIL.ExternalResolvers.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Nodes.Internal;
using RediSharp.RedIL.Utilities;
using RediSharp.Contracts;
using RediSharp.Enums;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace RediSharp.RedIL
{
    class CSharpCompiler
    {
        private ExternalResolversDictionary _externalResolvers;
        
        class State
        {
            public RedILNode LastIterativeNode { get; set; }

            public AstNode ParentNode { get; set; }

            public RedILNode ParentRedILNode { get; set; }

            public State ParentState { get; set; }

            public State NewState(AstNode currentNode, RedILNode currentRedILNode)
            {
                var lastNode = currentRedILNode ?? ParentRedILNode;
                var lastIterative = LastIterativeNode;
                
                if (currentRedILNode is DoWhileNode ||
                    currentRedILNode is WhileNode)
                {
                    lastIterative = currentRedILNode;
                }
                
                return new State()
                {
                    ParentState = this,
                    ParentNode = currentNode,
                    LastIterativeNode = lastIterative,
                    ParentRedILNode = lastNode
                };
            }
        }

        class AstVisitor : IAstVisitor<State, RedILNode>
        {
            private CSharpCompiler _compiler;
            
            private DecompilationResult _csharp;

            private long _labelNum;

            public AstVisitor(CSharpCompiler compiler, DecompilationResult csharp)
            {
                _compiler = compiler;
                _csharp = csharp;
                _labelNum = 0;
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
                var arrayTableDef = new ArrayTableDefinitionNode();
                arrayTableDef.Elements = arrayCreateExpression.Initializer.Elements.Select(elem =>
                    CastUtilities.CastRedILNode<ExpressionNode>(elem.AcceptVisitor(this, data.NewState(arrayCreateExpression, arrayTableDef)))).ToArray();

                return arrayTableDef;
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
                return asExpression.Expression.AcceptVisitor(this, data.NewState(asExpression, null));
            }
                
            public RedILNode VisitAssignmentExpression(AssignmentExpression assignmentExpression, State data)
            {
                if (data.ParentNode.NodeType != NodeType.Statement)
                {
                    throw new RedILException("Assigment is only possible within a statement");
                }

                var assignNode = new AssignNode();
                
                var left = CastUtilities.CastRedILNode<ExpressionNode>(assignmentExpression.Left.AcceptVisitor(this, data.NewState(assignmentExpression, assignNode)));
                var right = CastUtilities.CastRedILNode<ExpressionNode>(assignmentExpression.Right.AcceptVisitor(this, data.NewState(assignmentExpression, assignNode)));

                if (assignmentExpression.Operator == AssignmentOperatorType.Assign)
                {
                    assignNode.Left = left;
                    assignNode.Right = right;
                }
                else
                {
                    var op = OperatorUtilities.BinaryOperator(assignmentExpression.Operator);
                    assignNode.Left = left;
                    assignNode.Right = VisitBinaryOperatorExpression(left, right, op, data);
                }

                return assignNode;
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
                //TODO: send parent node
                var op = OperatorUtilities.BinaryOperator(binaryOperatorExpression.Operator);
                var left = CastUtilities.CastRedILNode<ExpressionNode>(binaryOperatorExpression.Left.AcceptVisitor(this, data.NewState(binaryOperatorExpression, null)));
                var right = CastUtilities.CastRedILNode<ExpressionNode>(binaryOperatorExpression.Right.AcceptVisitor(this, data.NewState(binaryOperatorExpression, null)));

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
                else if (op == BinaryExpressionOperator.NullCoalescing)
                {
                    return new BinaryExpressionNode(left.DataType, op, left, right);
                }

                throw new RedILException($"Unsupported operator '{op}' with data types '{left.DataType}' and '{right.DataType}'");
            }

            public RedILNode VisitBlockStatement(BlockStatement blockStatement, State data)
            {
                var block = new BlockNode();
                
                foreach (var child in blockStatement.Children)
                {
                    var visited = child.AcceptVisitor(this, data.NewState(blockStatement, block));
                    if (visited.Type != RedILNodeType.Empty)
                    {
                        block.Children.Add(visited);
                    }
                }

                return block;
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
                DataValueType resType;
                var type = castExpression.Type as PrimitiveType;

                resType = type is null ? DataValueType.Unknown : TypeUtilities.GetValueType(type.KnownTypeCode);

                var castNode = new CastNode(resType);
                castNode.Argument = CastUtilities.CastRedILNode<ExpressionNode>(castExpression.Expression.AcceptVisitor(this, data.NewState(castExpression, castNode)));

                return castNode.Argument.Type != RedILNodeType.Nil ? castNode : castNode.Argument;
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
                var conditional = new ConditionalExpressionNode();
                
                conditional.Condition = CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.Condition.AcceptVisitor(this, data.NewState(conditionalExpression, conditional)));
                conditional.IfYes = CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.TrueExpression.AcceptVisitor(this, data.NewState(conditionalExpression, conditional)));
                conditional.IfNo = CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.FalseExpression.AcceptVisitor(this, data.NewState(conditionalExpression, conditional)));

                return conditional;
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
                return new ContinueNode();
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
                var doWhile = new DoWhileNode();
                doWhile.Condition = CastUtilities.CastRedILNode<ExpressionNode>(doWhileStatement.Condition.AcceptVisitor(this, data.NewState(doWhileStatement, doWhile)));
                doWhile.Body = RemoveFirstLevelContinue(CastUtilities.CastRedILNode<BlockNode>(
                    doWhileStatement.EmbeddedStatement.AcceptVisitor(this, data.NewState(doWhileStatement, doWhile))), data);

                return doWhile;
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
                return expressionStatement.Expression.AcceptVisitor(this, data.NewState(expressionStatement, null));
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
                //TODO: Find how to handle for each loops over dictionaries
                // In case of for each over arrays, there is a 1=>1 map between the Lua variable and the C# variable,
                // In for each over dictionary, one C# variable (KeyValuePair) maps to 2 Lua variables
                var cursorType = ExtractTypeFromAnnontations(foreachStatement.Annotations);
                var iteratorLoop = new IteratorLoopNode();

                iteratorLoop.CursorType = cursorType;
                iteratorLoop.CursorName = foreachStatement.VariableName;
                iteratorLoop.Over = CastUtilities.CastRedILNode<ExpressionNode>(
                    foreachStatement.InExpression.AcceptVisitor(this, data.NewState(foreachStatement, iteratorLoop)));
                iteratorLoop.Body = CastUtilities.CastRedILNode<BlockNode>(foreachStatement.EmbeddedStatement.AcceptVisitor(this, data.NewState(foreachStatement, iteratorLoop)));

                return iteratorLoop;
            }

            public RedILNode VisitForStatement(ForStatement forStatement, State data)
            {
                var blockNode = new BlockNode()
                {
                    Explicit = false
                };

                foreach (var initializer in forStatement.Initializers)
                {
                    var visited = initializer.AcceptVisitor(this, data.NewState(forStatement, blockNode));
                    blockNode.Children.Add(visited);
                }

                var whileNode = new WhileNode();
                whileNode.Condition = CastUtilities.CastRedILNode<ExpressionNode>(forStatement.Condition.AcceptVisitor(this, data.NewState(forStatement, whileNode)));
                whileNode.Body = RemoveFirstLevelContinue(CastUtilities.CastRedILNode<BlockNode>(
                    forStatement.EmbeddedStatement.AcceptVisitor(this, data.NewState(forStatement, whileNode))), data);

                foreach (var iterator in forStatement.Iterators)
                {
                    var visited = iterator.AcceptVisitor(this, data.NewState(forStatement, whileNode));
                    whileNode.Body.Children.Add(visited);
                }
                
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

            public RedILNode VisitIfElseStatement(IfElseStatement ifElseStatement, State data)
            {
                var ifNode = new IfNode();
                ifNode.Condition = CastUtilities.CastRedILNode<ExpressionNode>(ifElseStatement.Condition.AcceptVisitor(this, data.NewState(ifElseStatement, ifNode)));
                ifNode.IfTrue = ifElseStatement.TrueStatement.AcceptVisitor(this, data.NewState(ifElseStatement, ifNode));
                ifNode.IfFalse = ifElseStatement.FalseStatement.AcceptVisitor(this, data.NewState(ifElseStatement, ifNode));

                if (ifNode.IfTrue is NilNode) ifNode.IfTrue = null;
                if (ifNode.IfFalse is NilNode) ifNode.IfFalse = null;

                return ifNode;
            }

            public RedILNode VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration, State data)
            {
                throw new System.NotImplementedException();
            }

            public RedILNode VisitIndexerExpression(IndexerExpression indexerExpression, State data)
            {
                //TODO: set parent node
                var target = CastUtilities.CastRedILNode<ExpressionNode>(indexerExpression.Target.AcceptVisitor(this, data.NewState(indexerExpression, null)));
                foreach (var arg in indexerExpression.Arguments)
                {
                    var argVisited = CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this, data.NewState(indexerExpression, null)));

                    // In LUA, array indices start at 1
                    if (target.DataType == DataValueType.Array && argVisited.DataType == DataValueType.Integer)
                    {
                        if (argVisited.Type == RedILNodeType.Constant)
                        {
                            argVisited = new ConstantValueNode(DataValueType.Integer,
                                int.Parse(((ConstantValueNode) argVisited).Value.ToString()) + 1);
                        }
                        else
                        {
                            argVisited = new BinaryExpressionNode(DataValueType.Integer, BinaryExpressionOperator.Add, argVisited, new ConstantValueNode(DataValueType.Integer, 1));
                        }
                    }
                    
                    target = new TableKeyAccessNode(target, argVisited);
                }

                return target;
            }

            public RedILNode VisitInterpolatedStringExpression(InterpolatedStringExpression interpolatedStringExpression, State data)
            {
                //TODO: set parent node
                var strings = new List<ExpressionNode>();
                foreach (var str in interpolatedStringExpression.Children)
                {
                    var child = CastUtilities.CastRedILNode<ExpressionNode>(str.AcceptVisitor(this, data.NewState(interpolatedStringExpression, null)));
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
                var expr = interpolation.Expression.AcceptVisitor(this, data.NewState(interpolation, null));
                return expr;
            }

            public RedILNode VisitInvocationExpression(InvocationExpression invocationExpression, State data)
            {
                var memberReference = invocationExpression.Target as MemberReferenceExpression;
                if (memberReference is null)
                {
                    throw new RedILException("Invocation is only possible by a member reference");
                }

                var isStatic = memberReference.Target is TypeReferenceExpression;
                
                var resolveResult =
                    invocationExpression.Annotations.FirstOrDefault(annot => annot is CSharpInvocationResolveResult) as
                        CSharpInvocationResolveResult;
                if (resolveResult is null)
                {
                    throw new RedILException($"Unable to find member resolve annotation");
                }
                
                //TODO: Consider caching
                var targetType = Type.GetType(resolveResult.TargetResult.Type.ReflectionName);
                var method = targetType.GetMethod(memberReference.MemberName,
                    (isStatic ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public);
                
                if (method is null)
                {
                    throw new RedILException(
                        $"Unable to find '{memberReference.MemberName}' member in '{resolveResult.TargetResult.Type.ReflectionName}'");
                }

                var redILResolveAttribute = method.CustomAttributes
                    .FirstOrDefault(attr => attr.AttributeType == typeof(RedILResolve));
                
                RedILResolver resolver;
                if (redILResolveAttribute is null)
                {
                    resolver = _compiler._externalResolvers.FindResolver(resolveResult.TargetResult.Type.ReflectionName,
                        resolveResult.TargetResult.Type.FullName, memberReference.MemberName,
                        EntryType.Method);

                    if (resolver is null)
                    {
                        throw new RedILException($"Could not find resolver for '{memberReference.MemberName}' of '{resolveResult.TargetResult.Type.ReflectionName}'");
                    }
                }
                else
                {
                    var resolverTypeArg = redILResolveAttribute.ConstructorArguments.First().Value;
                    var resolverCustomArgs =
                        (redILResolveAttribute.ConstructorArguments.Skip(1).First().Value as
                            ReadOnlyCollection<CustomAttributeTypedArgument>).Select(arg => arg.Value).ToArray();
                    var resolve = Activator.CreateInstance(redILResolveAttribute.AttributeType, resolverTypeArg, resolverCustomArgs) as RedILResolve;
                    resolver = resolve.CreateResolver();
                }
                
                var target = isStatic ? null : CastUtilities.CastRedILNode<ExpressionNode>(memberReference.Target.AcceptVisitor(this, data.NewState(invocationExpression, null)));
                var arguments = invocationExpression.Arguments.Select(arg =>
                    CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this,
                        data.NewState(invocationExpression, null)))).ToArray();

                return resolver.Resolve(null, target, arguments);
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
                var isStatic = memberReferenceExpression.Target is TypeReferenceExpression;

                var resolveResult =
                    memberReferenceExpression.Annotations.FirstOrDefault(annot => annot is MemberResolveResult) as
                        MemberResolveResult;
                if (resolveResult is null)
                {
                    throw new RedILException($"Unable to find member resolve annotation");
                }

                //TODO: Consider caching
                var targetType = Type.GetType(resolveResult.TargetResult.Type.ReflectionName);
                var members = targetType.GetMember(memberReferenceExpression.MemberName,
                    (isStatic ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public);

                var member = members.FirstOrDefault();
                if (member is null)
                {
                    throw new RedILException(
                        $"Unable to find '{memberReferenceExpression.MemberName}' member in '{resolveResult.TargetResult.Type.ReflectionName}'");
                }

                var redILResolveAttribute = member.CustomAttributes
                    .FirstOrDefault(attr => attr.AttributeType == typeof(RedILResolve));

                RedILResolver resolver;
                if (redILResolveAttribute is null)
                {
                    resolver = _compiler._externalResolvers.FindResolver(resolveResult.TargetResult.Type.ReflectionName,
                        resolveResult.TargetResult.Type.FullName, memberReferenceExpression.MemberName,
                        EntryType.Member);

                    if (resolver is null)
                    {
                        throw new RedILException($"Could not find resolver for '{memberReferenceExpression.MemberName}' of '{resolveResult.TargetResult.Type.ReflectionName}'");
                    }
                }
                else
                {
                    var resolverTypeArg = redILResolveAttribute.ConstructorArguments.First().Value;
                    var resolverCustomArgs =
                        (redILResolveAttribute.ConstructorArguments.Skip(1).First().Value as
                            ReadOnlyCollection<CustomAttributeTypedArgument>).Select(arg => arg.Value).ToArray();
                    var resolve = Activator.CreateInstance(redILResolveAttribute.AttributeType, resolverTypeArg, resolverCustomArgs) as RedILResolve;
                    resolver = resolve.CreateResolver();
                }
                
                var target = isStatic ? null : CastUtilities.CastRedILNode<ExpressionNode>(memberReferenceExpression.Target.AcceptVisitor(this, data.NewState(memberReferenceExpression, null)));

                return resolver.Resolve(null, target, null);
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
                return new NilNode();
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
                return parenthesizedExpression.Expression.AcceptVisitor(this, data.NewState(parenthesizedExpression, null));
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
                var returnNode = new ReturnNode();
                returnNode.Value = CastUtilities.CastRedILNode<ExpressionNode>(returnStatement.Expression.AcceptVisitor(this, data.NewState(returnStatement, returnNode)));
                return returnNode;
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
                var operand = CastUtilities.CastRedILNode<ExpressionNode>(unaryOperatorExpression.Expression.AcceptVisitor(this, data.NewState(unaryOperatorExpression, null)));
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
                    return new AssignNode(operand, VisitBinaryOperatorExpression(operand, constantOne, binaryOp, data.NewState(unaryOperatorExpression, null)));
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
                        variable.AcceptVisitor(this, data.NewState(variableDeclarationStatement, block)));
                    block.Children.Add(decl);
                }

                return block;
            }

            public RedILNode VisitVariableInitializer(VariableInitializer variableInitializer, State data)
            {
                var varDeclareNode = new VariableDeclareNode() { Name = variableInitializer.Name };
                varDeclareNode.Value = variableInitializer.Initializer != null
                    ? CastUtilities.CastRedILNode<ExpressionNode>(
                        variableInitializer.Initializer.AcceptVisitor(this, data.NewState(variableInitializer, varDeclareNode)))
                    : null;
                return varDeclareNode;
            }

            public RedILNode VisitWhileStatement(WhileStatement whileStatement, State data)
            {
                var whileNode = new WhileNode();
                
                whileNode.Condition =
                    CastUtilities.CastRedILNode<ExpressionNode>(
                        whileStatement.Condition.AcceptVisitor(this, data.NewState(whileStatement, whileNode)));
                whileNode.Body = RemoveFirstLevelContinue(CastUtilities.CastRedILNode<BlockNode>(
                    whileStatement.EmbeddedStatement.AcceptVisitor(this, data.NewState(whileStatement, whileNode))), data);

                return whileNode;
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

            private DataValueType ResolveExpressionType(Expression expr)
                => ExtractTypeFromAnnontations(expr.Annotations);

            private DataValueType ExtractTypeFromAnnontations(IEnumerable<object> annontations)
            {
                var resType = DataValueType.Unknown;
                var ilResolveResult = annontations.Where(annot => annot is ILVariableResolveResult)
                    .FirstOrDefault() as ILVariableResolveResult;

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

            //TODO: This covers the cases I've seen so far, might have to rewrite it to a more general version that would remove all instances of `continue`
            private BlockNode RemoveFirstLevelContinue(BlockNode node, State state)
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
        }

        public CSharpCompiler()
        {
            _externalResolvers = new ExternalResolversDictionary();
        }
        
        public RedILNode Compile(DecompilationResult csharp)
            => csharp.Body.AcceptVisitor(new AstVisitor(this, csharp), new State());
    }
}