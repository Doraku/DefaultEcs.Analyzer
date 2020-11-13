using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class ISymbolExtension
    {
        private static readonly ImmutableHashSet<string> _componentAttributes = ImmutableHashSet.Create(
            "DefaultEcs.System.DisabledAttribute",
            "DefaultEcs.System.ComponentAttribute",
            "DefaultEcs.System.WithAttribute",
            "DefaultEcs.System.WithEitherAttribute",
            "DefaultEcs.System.WithoutAttribute",
            "DefaultEcs.System.WithoutEitherAttribute",
            "DefaultEcs.System.WhenAddedAttribute",
            "DefaultEcs.System.WhenAddedEitherAttribute",
            "DefaultEcs.System.WhenChangedAttribute",
            "DefaultEcs.System.WhenChangedEitherAttribute",
            "DefaultEcs.System.WhenRemovedAttribute",
            "DefaultEcs.System.WhenRemovedEitherAttribute");

        public static IEnumerable<AttributeData> GetComponentAttributes(this ISymbol symbol) => symbol.GetAttributes().Where(a => _componentAttributes.Contains(a.ToString()));

        public static bool HasComponentAttribute(this ISymbol symbol) => symbol.GetComponentAttributes().Any();

        public static bool HasSubscribeAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.SubscribeAttribute");

        public static bool HasWithPredicateAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.System.WithPredicateAttribute");

        public static bool HasUpdateAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.System.UpdateAttribute");
    }
}
