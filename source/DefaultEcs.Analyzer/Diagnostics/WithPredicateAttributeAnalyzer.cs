using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class WithPredicateAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor InvalidSignatureRule = new DiagnosticDescriptor(
            "DEA0002",
            "WithPredicateAttribute used on an invalid method",
            "Remove WithPredicateAttribute from the '{0}' method or change the method signature",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Error,
            true,
            "WithPredicateAttribute should only be used on method with the ComponentPredicate signature.");

        public static readonly DiagnosticDescriptor InvalidBaseTypeRule = new DiagnosticDescriptor(
            "DEA0003",
            "WithPredicateAttribute used on a method which is not a member of DefaultEcs.System.AEntitySystem, DefaultEcs.System.AEntitiesSystem, DefaultEcs.System.AEntityBufferedSystem or DefaultEcs.System.AEntitiesBufferedSystem",
            "Remove WithPredicateAttribute from the '{0}' method",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "WithPredicateAttribute should only be used on method member of DefaultEcs.System.AEntitySystem, DefaultEcs.System.AEntitiesSystem, DefaultEcs.System.AEntityBufferedSystem or DefaultEcs.System.AEntitiesBufferedSystem.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InvalidSignatureRule, InvalidBaseTypeRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is IMethodSymbol method && method.HasWithPredicateAttribute())
            {
                if (!method.ContainingType.IsEntitySystem())
                {
                    context.ReportDiagnostic(Diagnostic.Create(InvalidBaseTypeRule, method.Locations[0], method.Name));
                }
                else if (method.ReturnType.SpecialType != SpecialType.System_Boolean || method.Parameters.Length != 1 || method.Parameters[0].RefKind != RefKind.In)
                {
                    context.ReportDiagnostic(Diagnostic.Create(InvalidSignatureRule, method.Locations[0], method.Name));
                }
            }
        }
    }
}
