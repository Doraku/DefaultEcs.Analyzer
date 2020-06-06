using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class INamedTypeSymbolExtension
    {
        private static readonly ImmutableHashSet<string> _entitySystemTypes = ImmutableHashSet.Create(
            "DefaultEcs.System.AEntitySystem<T>",
            "DefaultEcs.System.AEntityBufferedSystem<T>");

        public static bool IsEntitySystem(this INamedTypeSymbol type) => !(type is null) && (_entitySystemTypes.Contains(type.ConstructedFrom.ToString()) || type.BaseType.IsEntitySystem());
    }
}
