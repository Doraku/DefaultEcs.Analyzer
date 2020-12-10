using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class INamedTypeSymbolExtension
    {
        private static readonly ImmutableHashSet<string> _entitySystemTypes = ImmutableHashSet.Create(
            "DefaultEcs.System.AEntitySystem<T>",
            "DefaultEcs.System.AEntityBufferedSystem<T>",
            "DefaultEcs.System.AEntitiesSystem<TState, TKey>",
            "DefaultEcs.System.AEntitiesBufferedSystem<TState, TKey>");

        public static bool IsEntitySystem(this INamedTypeSymbol type) => !(type is null) && (_entitySystemTypes.Contains(type.ConstructedFrom.ToString()) || type.BaseType.IsEntitySystem());

        public static bool IsAEntitySystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is "DefaultEcs.System.AEntitySystem<T>")
            {
                genericTypes = type.TypeArguments;
                return true;
            }

            genericTypes = null;

            return type?.BaseType.IsAEntitySystem(out genericTypes) is true;
        }

        public static bool IsAEntityBufferedSystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is "DefaultEcs.System.AEntityBufferedSystem<T>")
            {
                genericTypes = type.TypeArguments;
                return true;
            }

            genericTypes = null;

            return type?.BaseType.IsAEntityBufferedSystem(out genericTypes) is true;
        }

        public static bool IsAEntitiesSystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is "DefaultEcs.System.AEntitiesSystem<TState, TKey>")
            {
                genericTypes = type.TypeArguments;
                return true;
            }

            genericTypes = null;

            return type?.BaseType.IsAEntitiesSystem(out genericTypes) is true;
        }

        public static bool IsAEntitiesBufferedSystem(this INamedTypeSymbol type, out IList<ITypeSymbol> genericTypes)
        {
            if (type?.ConstructedFrom.ToString() is "DefaultEcs.System.AEntitiesBufferedSystem<TState, TKey>")
            {
                genericTypes = type.TypeArguments;
                return true;
            }

            genericTypes = null;

            return type?.BaseType.IsAEntitiesBufferedSystem(out genericTypes) is true;
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
