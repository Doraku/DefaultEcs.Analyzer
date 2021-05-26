using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Suppressors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class WithPredicateAttributeSuppressor : DiagnosticSuppressor
    {
        public static readonly SuppressionDescriptor UnusedRule = new(
            "DES0002",
            "IDE0051",
            "Private member is used by reflection.");

        public static readonly SuppressionDescriptor InRule = new(
            "DES0004",
            "RCS1242",
            "Signature is dictated by its usage as a ComponentPredicate.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(UnusedRule, InRule);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                if (context.TryGetMethodSymbol(diagnostic, out IMethodSymbol method) && method.HasWithPredicateAttribute())
                {
                    context.ReportSuppression(diagnostic, UnusedRule);
                    context.ReportSuppression(diagnostic, InRule);
                }
            }
        }
    }
}
