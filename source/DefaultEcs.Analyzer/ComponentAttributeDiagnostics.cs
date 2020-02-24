using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ComponentAttributeAnalyser : DiagnosticAnalyzer
    {
        private static readonly ImmutableHashSet<string> _supportedTypes = ImmutableHashSet.Create(
            "DefaultEcs.System.AEntitySystem<T>",
            "DefaultEcs.System.AEntityBufferedSystem<T>");

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

        public static readonly DiagnosticDescriptor InvalidBaseTypeRule = new DiagnosticDescriptor(
            "DEA0004",
            "Component attribute used on a type which is not derived from DefaultEcs.System.AEntitySystem or DefaultEcs.System.AEntityBufferedSystem",
            "Remove '{1}' from the '{0}' type.",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "Component attribute should only be used on type which is not derived from DefaultEcs.System.AEntitySystem or DefaultEcs.System.AEntityBufferedSystem.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InvalidBaseTypeRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static bool IsDerivedFromCorrectType(INamedTypeSymbol type) => type is null ? false : (_supportedTypes.Contains(type.ConstructedFrom.ToString()) || IsDerivedFromCorrectType(type.BaseType));

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is INamedTypeSymbol type
                && type.GetAttributes().Any(a => _componentAttributes.Contains(a.ToString()))
                && !IsDerivedFromCorrectType(type))
            {
                foreach (AttributeData attribute in type.GetAttributes().Where(a => _componentAttributes.Contains(a.ToString())))
                {
                    context.ReportDiagnostic(Diagnostic.Create(InvalidBaseTypeRule, type.Locations[0], type.Name, attribute.AttributeClass.Name));
                }
            }
        }
    }
}
