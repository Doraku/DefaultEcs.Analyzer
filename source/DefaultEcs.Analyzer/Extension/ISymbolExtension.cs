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

        public static IEnumerable<AttributeData> GetComponentAttributes(this ISymbol symbol) => symbol.GetAttributes().Where(a => _componentAttributes.Contains(a.AttributeClass.ToString()));

        public static bool HasComponentAttribute(this ISymbol symbol) => symbol.GetComponentAttributes().Any();

        public static bool HasDisabledAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.DisabledAttribute");

        public static bool HasSubscribeAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.SubscribeAttribute");

        public static bool HasWithPredicateAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.System.WithPredicateAttribute");

        public static bool HasUpdateAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.System.UpdateAttribute");

        public static bool HasAddedAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.System.AddedAttribute");

        public static bool HasChangedAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "DefaultEcs.System.ChangedAttribute");

        public static bool HasCompilerGeneratedAttribute(this ISymbol symbol) => symbol.GetAttributes().Any(a => a.ToString() == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");

        public static string GetNamespace(this ISymbol symbol)
        {
            INamespaceSymbol namespaceSymbol = symbol.ContainingNamespace;
            string value = string.Empty;

            while (namespaceSymbol?.IsGlobalNamespace is false)
            {
                value = namespaceSymbol.Name + (string.IsNullOrEmpty(value) ? value : ('.' + value));
                namespaceSymbol = namespaceSymbol.ContainingNamespace;
            }

            return value;
        }
    }
}
