using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extensions
{
    internal static class INamedTypeSymbolExtension
    {
        private const string AEntitySetSystem = "DefaultEcs.System.AEntitySetSystem<T>";
        private const string AEntitySortedSetSystem = "DefaultEcs.System.AEntitySortedSetSystem<TState, TComponent>";
        private const string AEntityMultiMapSystem = "DefaultEcs.System.AEntityMultiMapSystem<TState, TKey>";

        private static readonly ImmutableHashSet<string> _entitySystemTypes = ImmutableHashSet.Create(
            AEntitySetSystem,
            AEntitySortedSetSystem,
            AEntityMultiMapSystem);

        public static bool IsEntitySystem(this INamedTypeSymbol type) => type is not null && (_entitySystemTypes.Contains(type.ConstructedFrom.ToString()) || type.BaseType.IsEntitySystem());

        public static bool IsAEntitySetSystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is AEntitySetSystem)
            {
                genericTypes = type.TypeArguments;
                return true;
            }

            genericTypes = null;

            return type?.BaseType.IsAEntitySetSystem(out genericTypes) is true;
        }

        public static bool IsAEntitySortedSetSystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is AEntitySortedSetSystem)
            {
                genericTypes = type.TypeArguments;
                return true;
            }

            genericTypes = null;

            return type?.BaseType.IsAEntitySortedSetSystem(out genericTypes) is true;
        }

        public static bool IsAEntityMultiMapSystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is AEntityMultiMapSystem)
            {
                genericTypes = type.TypeArguments;
                return true;
            }

            genericTypes = null;

            return type?.BaseType.IsAEntityMultiMapSystem(out genericTypes) is true;
        }

        public static IEnumerable<INamedTypeSymbol> GetParentTypes(this INamedTypeSymbol type)
        {
            while (type != null)
            {
                yield return type;
                type = type.ContainingType;
            }
        }

        public static string GetName(this INamedTypeSymbol type) => type.Name + (type.TypeParameters.Length > 0 ? $"<{string.Join(", ", type.TypeParameters)}>" : string.Empty);
    }
}
