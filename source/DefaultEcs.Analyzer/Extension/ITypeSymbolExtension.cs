using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class ITypeSymbolExtension
    {
        public static bool IsWorld(this ITypeSymbol type) => type?.ToString() == "DefaultEcs.World";

        public static bool IsEntity(this ITypeSymbol type) => type?.ToString() == "DefaultEcs.Entity";

        public static bool IsEntityRecord(this ITypeSymbol type) => type?.ToString() == "DefaultEcs.EntityRecord";

        public static bool IsEntityCommandRecorder(this ITypeSymbol type) => type?.ToString() == "DefaultEcs.Command.EntityCommandRecorder";

        public static bool IsComponents(this ITypeSymbol type) => type?.ToString().StartsWith("DefaultEcs.Components<") is true;

        public static bool IsIParallelRunner(this ITypeSymbol type) => type?.ToString() == "DefaultEcs.Threading.IParallelRunner";

        public static bool IsPartial(this ITypeSymbol type) => type.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<TypeDeclarationSyntax>().Any(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

        public static bool HasTypeParameter(this ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.TypeParameter
                || (type is INamedTypeSymbol namedType && namedType.TypeArguments.Any(t => t.HasTypeParameter()))
                || (type is IArrayTypeSymbol arrayType && arrayType.ElementType.HasTypeParameter());
        }
    }
}
