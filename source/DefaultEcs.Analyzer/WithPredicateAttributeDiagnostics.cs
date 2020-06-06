using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class WithPredicateAttributeAnalyser : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor InvalidSignatureRule = new DiagnosticDescriptor(
            "DEA0002",
            "WithPredicateAttribute used on an invalid method",
            "Remove WithPredicateAttribute from the '{0}' method or change the method signature.",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Error,
            true,
            "WithPredicateAttribute should only be used on method with the ComponentPredicate signature.");

        public static readonly DiagnosticDescriptor InvalidBaseTypeRule = new DiagnosticDescriptor(
            "DEA0003",
            "WithPredicateAttribute used on a method which is not a member of DefaultEcs.System.AEntitySystem or DefaultEcs.System.AEntityBufferedSystem",
            "Remove WithPredicateAttribute from the '{0}' method.",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "WithPredicateAttribute should only be used on method member of DefaultEcs.System.AEntitySystem or DefaultEcs.System.AEntityBufferedSystem.");

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

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class WithPredicateAttributeSuppressor : DiagnosticSuppressor
    {
        public static readonly SuppressionDescriptor UnusedRule = new SuppressionDescriptor(
            "DES0002",
            "IDE0051",
            "Private member is used by reflection.");

        public static readonly SuppressionDescriptor InRule = new SuppressionDescriptor(
            "DES0004",
            "RCS1242",
            "Signature is dictated by its usage as a ComponentPredicate.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(UnusedRule, InRule);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                if (diagnostic.TryGetMethodSymbol(context, out IMethodSymbol method) && method.HasWithPredicateAttribute())
                {
                    if (diagnostic.Id == UnusedRule.SuppressedDiagnosticId)
                    {
                        context.ReportSuppression(Suppression.Create(UnusedRule, diagnostic));
                    }
                    else if (diagnostic.Id == InRule.SuppressedDiagnosticId)
                    {
                        context.ReportSuppression(Suppression.Create(InRule, diagnostic));
                    }
                }
            }
        }
    }
}
