using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Extension
{
    public static class IMethodSymbolExtension
    {
        private static readonly ImmutableHashSet<string> _entityMethods = ImmutableHashSet.Create(
            "Get",
            "Set",
            "SetSameAs",
            "Remove",
            "Enable",
            "Disable",
            "NotifyChanged",
            "Dispose",
            "SetAsChildOf",
            "SetAsParentOf",
            "RemoveFromChildrenOf",
            "RemoveFromParentsOf");

        public static bool IsAEntitySystemOverride(this IMethodSymbol method)
        {
            return method.ContainingType.IsAEntitySystem(out ITypeSymbol stateType)
                && method.IsOverride
                && method.Name == "Update"
                && method.ReturnsVoid
                && method.Parameters.Length == 2
                && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, stateType)
                && (
                    (method.Parameters[1].RefKind == RefKind.In && method.Parameters[1].Type.ToString() == "DefaultEcs.Entity")
                    || (method.Parameters[1].RefKind == RefKind.None && method.Parameters[1].Type.ToString() == "System.ReadOnlySpan<DefaultEcs.Entity>"));
        }

        public static IEnumerable<(InvocationExpressionSyntax, IMethodSymbol)> GetEntityInvocations(this IMethodSymbol method, SymbolAnalysisContext context)
        {
            foreach (InvocationExpressionSyntax invocation in method.DeclaringSyntaxReferences.SelectMany(r => r.GetSyntax().DescendantNodes().OfType<InvocationExpressionSyntax>()))
            {
                IMethodSymbol invokedMethod = context.Compilation.GetSemanticModel(invocation.SyntaxTree)?.GetSymbolInfo(invocation.Expression).As<IMethodSymbol>();
                if (invokedMethod?.ContainingType.IsEntity() is true && _entityMethods.Contains(invokedMethod.Name))
                {
                    yield return (invocation, invokedMethod);
                }
            }
        }
    }
}
