using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class INamedTypeSymbolExtension
    {
        private static readonly ImmutableHashSet<string> _entitySystemTypes = ImmutableHashSet.Create(
            "DefaultEcs.System.AEntitySetSystem<T>",
            "DefaultEcs.System.AEntityMultiMapSystem<TState, TKey>");

        public static bool IsEntitySystem(this INamedTypeSymbol type) => !(type is null) && (_entitySystemTypes.Contains(type.ConstructedFrom.ToString()) || type.BaseType.IsEntitySystem());

        public static bool IsAEntitySetSystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is "DefaultEcs.System.AEntitySetSystem<T>")
            {
                genericTypes = type.TypeArguments;
                return true;
            }

            genericTypes = null;

            return type?.BaseType.IsAEntitySetSystem(out genericTypes) is true;
        }

        public static bool IsAEntityMultiMapSystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is "DefaultEcs.System.AEntityMultiMapSystem<TState, TKey>")
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
