using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class INamedTypeSymbolExtension
    {
        private static readonly ImmutableHashSet<string> _entitySystemTypes = ImmutableHashSet.Create(
            "DefaultEcs.System.AEntitySystem<T>",
            "DefaultEcs.System.AEntityBufferedSystem<T>");

        public static bool IsEntity(this INamedTypeSymbol type) => type?.ToString() == "DefaultEcs.Entity";

        public static bool IsEntitySystem(this INamedTypeSymbol type) => !(type is null) && (_entitySystemTypes.Contains(type.ConstructedFrom.ToString()) || type.BaseType.IsEntitySystem());

        public static bool IsAEntitySystem(this INamedTypeSymbol type, out ITypeSymbol stateType)
        {
            if (type?.ConstructedFrom.ToString() == "DefaultEcs.System.AEntitySystem<T>")
            {
                stateType = type.TypeArguments[0];
                return true;
            }

            stateType = null;

            return type?.BaseType.IsAEntitySystem(out stateType) == true;
        }
    }
}
