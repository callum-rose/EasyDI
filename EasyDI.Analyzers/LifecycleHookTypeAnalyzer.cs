using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyDI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LifecycleHookTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.LifecycleHookGenericTypeArg,
        "Invalid lifecycle hook type",
        "Type '{0}' must implement ILifecycleHook or IDisposable",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Lifecycle hooks must implement ILifecycleHook or IDisposable.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var genericName = memberAccess.Name as GenericNameSyntax;
        if (genericName?.Identifier.Text != "RegisterLifecycleHook")
        {
            return;
        }

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Verify it's the extension method from IObjectRegistryExtensions
        if (methodSymbol.ContainingType.ToDisplayString() != "EasyDI.LifecycleHooks.IObjectRegistryExtensions")
        {
            return;
        }

        if (methodSymbol.TypeArguments.Length != 1)
        {
            return;
        }

        var typeArgument = methodSymbol.TypeArguments[0];

        var lifecycleHookType = context.Compilation.GetTypeByMetadataName("EasyDI.LifecycleHooks.ILifecycleHook");
        var disposableType = context.Compilation.GetTypeByMetadataName("System.IDisposable");

        if (lifecycleHookType == null || disposableType == null)
        {
            return;
        }

        bool implementsLifecycleHook =
            typeArgument.AllInterfaces.Contains(lifecycleHookType, SymbolEqualityComparer.Default) ||
            SymbolEqualityComparer.Default.Equals(typeArgument, lifecycleHookType);
        bool implementsDisposable =
            typeArgument.AllInterfaces.Contains(disposableType, SymbolEqualityComparer.Default) ||
            SymbolEqualityComparer.Default.Equals(typeArgument, disposableType);

        if (implementsLifecycleHook || implementsDisposable)
        {
            return;
        }

        var typeSyntax = genericName.TypeArgumentList.Arguments[0];
        var diagnostic = Diagnostic.Create(Rule, typeSyntax.GetLocation(), typeArgument.Name);
        context.ReportDiagnostic(diagnostic);
    }
}