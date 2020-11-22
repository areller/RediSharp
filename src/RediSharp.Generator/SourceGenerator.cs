using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RediSharp.Generator.Diagnostics;
using RediSharp.Generator.Diagnostics.Messages;
using RediSharp.Generator.Resolving;
using RediSharp.Generator.Resolving.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RediSharp.Generator
{
    [Generator]
    class SourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is SyntaxReceiver receiver)
            {
                var syntaxTreeSemanticModels = new Dictionary<SyntaxTree, SemanticModel>();

                int order = 0;
                foreach (var procedureCandidate in receiver.RedisProcedureCandidates)
                {
                    var treeSemantics = syntaxTreeSemanticModels.GetOrAdd(procedureCandidate.SyntaxTree, tree => context.Compilation.GetSemanticModel(procedureCandidate.SyntaxTree));
                    ProcessProcedureCandidate(context, procedureCandidate, treeSemantics, ref order);
                }
            }
        }

        private void ProcessProcedureCandidate(GeneratorExecutionContext context, ClassDeclarationSyntax procedureCandidate, SemanticModel semanticModel, ref int order)
        {
            var procedureDeclrSemantics = semanticModel.GetDeclaredSymbol(procedureCandidate)!;
            var attributes = procedureDeclrSemantics.GetAttributes();

            if (attributes.Any(attr => TypeUtilities.SymbolSameAsType(attr.AttributeClass!, typeof(DebugProcedureGeneratorAttribute))))
            {
                Debugger.Launch();
            }

            if (!attributes.Any(attr => TypeUtilities.SymbolSameAsType(attr.AttributeClass!, typeof(RedisProcedureAttribute))))
            {
                return;
            }

            var error = false;

            if (!procedureCandidate.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.Report<RedisProceduresMustBePartial>(Location.Create(procedureCandidate.SyntaxTree, procedureCandidate.Span));
                error = true;
            }

            if (!(procedureDeclrSemantics is INamedTypeSymbol nameSemantics &&
                nameSemantics.AllInterfaces.Any(intr => TypeUtilities.SymbolSameAsType(intr.OriginalDefinition, typeof(IRedisProcedure<>)))))
            {
                context.Report<RedisProceduresMustImplementInterface>(Location.Create(procedureCandidate.SyntaxTree, procedureCandidate.Span));
                error = true;
            }

            if (error)
            {
                return;
            }

            ProcessProcedure(context, procedureCandidate, procedureDeclrSemantics, semanticModel, ref order);
        }

        private void ProcessProcedure(GeneratorExecutionContext context, ClassDeclarationSyntax procedureDeclr, INamedTypeSymbol procedureDeclrSemantics, SemanticModel semanticModel, ref int order)
        {
            var defineMethod = (procedureDeclr.Members
                .Single(m =>
                {
                    if (!(m is MethodDeclarationSyntax methodDeclr &&
                        semanticModel.GetDeclaredSymbol(methodDeclr) is IMethodSymbol methodSemantics))
                    {
                        return false;
                    }

                    return true;
                }) as MethodDeclarationSyntax)!;

            if (defineMethod.Body is null)
            {
                context.Report<ArrowExpressionsNotSupported>(Location.Create(defineMethod.SyntaxTree, defineMethod.Span));
                return;
            }

            GenerateExecutionMethod(context, procedureDeclr, procedureDeclrSemantics, "Hello world! " + order, ref order);

            return;
        }

        private void GenerateExecutionMethod(GeneratorExecutionContext context, ClassDeclarationSyntax procedureDeclr, INamedTypeSymbol procedureDeclrSemantics, string lua, ref int order)
        {
            var sourceBuilder = new StringBuilder(@"
using StackExchange.Redis;
namespace " + procedureDeclrSemantics.ContainingNamespace.ToDisplayString() + @"
{
    public partial class " + procedureDeclrSemantics.Name + @"
    {
        public static string GetLua() => " + "@\"" + lua + "\";" + @"
    }
}");

            context.AddSource($"RediSharpGeneratedSource_{order}", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

            order++;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> RedisProcedureCandidates { get; private set; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclr && classDeclr.AttributeLists.Count > 0)
                {
                    RedisProcedureCandidates.Add(classDeclr);
                }
            }
        }
    }
}
