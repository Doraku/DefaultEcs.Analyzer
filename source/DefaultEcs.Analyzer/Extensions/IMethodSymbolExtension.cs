using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extensions
{
    internal static class IMethodSymbolExtension
    {
        private static readonly ImmutableHashSet<string> _updateParameters = ImmutableHashSet.Create(
            "DefaultEcs.Entity",
            "System.ReadOnlySpan<DefaultEcs.Entity>");

        public static bool IsEntitySystemUpdateOverride(this IMethodSymbol method, IList<ITypeSymbol> genericTypes = null)
        {
            return (genericTypes != null
                    || method.ContainingType.IsAEntitySetSystem(out genericTypes)
                    || method.ContainingType.IsAEntitySortedSetSystem(out genericTypes)
                    || method.ContainingType.IsAEntityMultiMapSystem(out genericTypes))
                && method.IsOverride
                && method.Name == "Update"
                && method.ReturnsVoid
                && method.Parameters.Length == genericTypes.Count + 1
                && genericTypes.Select((p, i) => SymbolEqualityComparer.Default.Equals(method.Parameters[i].Type, p)).All(b => b is true)
                && _updateParameters.Contains(method.Parameters[genericTypes.Count].Type.ToString());
        }
    }
}
