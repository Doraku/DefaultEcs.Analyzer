using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Extension
{
    public static class DiagnosticExtension
    {
        public static bool TryGetMethodSymbol(this Diagnostic diagnostic, SuppressionAnalysisContext context, out IMethodSymbol methodSymbol)
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
    }
}
