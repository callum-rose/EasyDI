using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REContainer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RegisterInstanceGenericOnlyAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticIds.RegisterInstanceGenericOnly,
            "RegisterInstance must use explicit generic type argument",
            "Use either RegisterInstance<T>(instance) or RegisterInstance(instance).As<T>()",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "RegisterInstance must be called with an explicit type argument or with As<T>().");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                // Check if it's RegisterInstance method
                var isRegisterInstance = memberAccess.Name switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.ValueText == "RegisterInstance",
                    IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText == "RegisterInstance",
                    _ => false
                };

                if (isRegisterInstance && memberAccess.Name is IdentifierNameSyntax)
                {
                    // Check if this is part of a fluent API chain (e.g., RegisterInstance(x).As<T>())
                    if (IsPartOfFluentChain(invocation))
                        return;

                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule,
                        memberAccess.Name.GetLocation()));
                }
            }
        }

        private static bool IsPartOfFluentChain(InvocationExpressionSyntax invocation)
        {
            if (!(invocation.Parent is MemberAccessExpressionSyntax parentMemberAccess))
                return false;

            // Check if the chained method is "As" (either generic or with Type parameter)
            if (parentMemberAccess.Name is GenericNameSyntax genericName &&
                genericName.Identifier.ValueText == "As")
                return true;

            // Check if it's As(typeof(...)) - the parent will be InvocationExpressionSyntax
            if (parentMemberAccess.Name is IdentifierNameSyntax identifierName &&
                identifierName.Identifier.ValueText == "As" &&
                parentMemberAccess.Parent is InvocationExpressionSyntax)
                return true;

            return false;
        }

    }
}
