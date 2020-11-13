using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SubscribeAttributeSuppressor : DiagnosticSuppressor
    {
        public static readonly SuppressionDescriptor UnusedRule = new SuppressionDescriptor(
            "DES0001",
            "IDE0051",
            "Private member is used by reflection.");

        public static readonly SuppressionDescriptor InRule = new SuppressionDescriptor(
            "DES0003",
            "RCS1242",
            "Signature is dictated by IPublisher.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(UnusedRule, InRule);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                if (context.TryGetMethodSymbol(diagnostic, out IMethodSymbol method) && method.HasSubscribeAttribute())
                {
                    context.ReportSuppression(diagnostic, UnusedRule);
                    context.ReportSuppression(diagnostic, InRule);
                }
            }
        }
    }
}
