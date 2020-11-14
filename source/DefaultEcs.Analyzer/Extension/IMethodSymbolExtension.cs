using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class IMethodSymbolExtension
    {
        private static readonly ImmutableHashSet<string> _updateParameters = ImmutableHashSet.Create(
            "DefaultEcs.Entity",
            "System.ReadOnlySpan<DefaultEcs.Entity>");

        public static bool IsEntitySystemUpdateOverride(this IMethodSymbol method, IList<ITypeSymbol> genericTypes = null)
        {
            return (genericTypes != null
                    || method.ContainingType.IsAEntitySystem(out genericTypes)
                    || method.ContainingType.IsAEntitiesSystem(out genericTypes)
                    || method.ContainingType.IsAEntityBufferedSystem(out genericTypes)
                    || method.ContainingType.IsAEntitiesBufferedSystem(out genericTypes))
                && method.IsOverride
                && method.Name == "Update"
                && method.ReturnsVoid
                && method.Parameters.Length == genericTypes.Count + 1
                && genericTypes.Select((p, i) => SymbolEqualityComparer.Default.Equals(method.Parameters[i].Type, p)).All(b => b is true)
                && _updateParameters.Contains(method.Parameters[genericTypes.Count].Type.ToString());
        }
    }
}
