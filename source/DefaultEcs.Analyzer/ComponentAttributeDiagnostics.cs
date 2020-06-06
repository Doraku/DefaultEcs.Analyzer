using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ComponentAttributeAnalyser : DiagnosticAnalyzer
    {
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

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is INamedTypeSymbol type
                && type.HasComponentAttribute()
                && !type.IsEntitySystem())
            {
                foreach (AttributeData attribute in type.GetComponentAttributes())
                {
                    context.ReportDiagnostic(InvalidBaseTypeRule, type.Locations[0], type.Name, attribute.AttributeClass.Name);
                }
            }
        }
    }
}
