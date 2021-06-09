using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Extensions
{
    internal static class SuppressionAnalysisContextExtension
    {
        public static bool TryGetTypeSymbol(this SuppressionAnalysisContext context, Diagnostic diagnostic, out ITypeSymbol typeSymbol)
        {
            SyntaxNode syntaxNode = diagnostic.Location.SourceTree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan) switch
            {
                TypeDeclarationSyntax type => type,
                _ => default
            };

            typeSymbol = syntaxNode is null ? null : context.GetSemanticModel(diagnostic.Location.SourceTree).GetDeclaredSymbol(syntaxNode) as ITypeSymbol;

            return typeSymbol != null;
        }

        public static bool TryGetMethodSymbol(this SuppressionAnalysisContext context, Diagnostic diagnostic, out IMethodSymbol methodSymbol)
        {
            SyntaxNode syntaxNode = diagnostic.Location.SourceTree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan) switch
            {
                ParameterSyntax parameter => parameter.Parent.Parent,
                MethodDeclarationSyntax methodDeclaration => methodDeclaration,
                _ => default
            };

            methodSymbol = syntaxNode is null ? null : context.GetSemanticModel(diagnostic.Location.SourceTree).GetDeclaredSymbol(syntaxNode) as IMethodSymbol;

            return methodSymbol != null;
        }

        public static void ReportSuppression(this SuppressionAnalysisContext context, Diagnostic diagnostic, SuppressionDescriptor descriptor)
        {
            if (diagnostic.Id == descriptor.SuppressedDiagnosticId)
            {
                context.ReportSuppression(Suppression.Create(descriptor, diagnostic));
            }
        }
    }
}
