using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class ITypeSymbolExtension
    {
        public static bool IsEntity(this ITypeSymbol type) => type?.ToString() == "DefaultEcs.Entity";

        public static bool IsPartial(this ITypeSymbol type) => type.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<TypeDeclarationSyntax>().Any(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }
}
