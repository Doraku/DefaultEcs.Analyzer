using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ComponentAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor InvalidBaseTypeRule = new DiagnosticDescriptor(
            "DEA0004",
            "Component attribute used on a type which is not derived from DefaultEcs.System.AEntitySystem, DefaultEcs.System.AEntitiesSystem, DefaultEcs.System.AEntityBufferedSystem or DefaultEcs.System.AEntitiesBufferedSystem",
            "Remove '{1}' from the '{0}' type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "Component attribute should only be used on type which is derived from DefaultEcs.System.AEntitySystem, DefaultEcs.System.AEntitiesSystem, DefaultEcs.System.AEntityBufferedSystem or DefaultEcs.System.AEntitiesBufferedSystem.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InvalidBaseTypeRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is INamedTypeSymbol type
                && type.HasComponentAttribute()
                && !type.IsEntitySystem())
            {
                foreach (AttributeData attribute in type.GetComponentAttributes())
                {
                    context.ReportDiagnostic(Diagnostic.Create(InvalidBaseTypeRule, type.Locations[0], type.Name, attribute.AttributeClass.Name));
                }
            }
        }
    }
}
