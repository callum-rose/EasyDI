using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyDI.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegisterInstanceGenericOnlyCodeFixProvider)), Shared]
    public class RegisterInstanceGenericOnlyCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(RegisterInstanceGenericOnlyAnalyzer.Rule.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var memberAccess = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<MemberAccessExpressionSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add explicit type argument",
                    createChangedDocument: c => AddTypeArgumentAsync(context.Document, memberAccess, c),
                    equivalenceKey: "AddExplicitTypeArgument"),
                diagnostic);
        }

        private async Task<Document> AddTypeArgumentAsync(Document document, MemberAccessExpressionSyntax memberAccess,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var invocation = memberAccess.Parent as InvocationExpressionSyntax;

            if (invocation == null || invocation.ArgumentList.Arguments.Count == 0)
                return document;

            // Get the type of the first argument
            var argumentType = semanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression, cancellationToken).Type;

            if (argumentType == null)
                return document;

            // Create generic name with type argument using minimal display string
            var typeArgument = SyntaxFactory.ParseTypeName(argumentType.ToMinimalDisplayString(semanticModel, memberAccess.SpanStart));
            var genericName = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("RegisterInstance"),
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(typeArgument)));

            var newMemberAccess = memberAccess.WithName(genericName);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(memberAccess, newMemberAccess);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
