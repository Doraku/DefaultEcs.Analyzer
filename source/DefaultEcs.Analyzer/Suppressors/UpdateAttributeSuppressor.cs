using System.Collections.Immutable;
using System.Linq;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Suppressors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UpdateAttributeSuppressor : DiagnosticSuppressor
    {
        public static readonly SuppressionDescriptor PartialRule = new SuppressionDescriptor(
            "DES0005",
            "RCS1043",
            "Partial class generated.");

        public static readonly SuppressionDescriptor UnusedRule = new SuppressionDescriptor(
            "DES0006",
            "IDE0051",
            "Partial class generated.");

        public static readonly SuppressionDescriptor NonReadOnlyStructAsReadOnlyReferenceRule = new SuppressionDescriptor(
            "DES0007",
            "RCS1242",
            "More explicit.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(PartialRule, UnusedRule, NonReadOnlyStructAsReadOnlyReferenceRule);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                if (context.TryGetTypeSymbol(diagnostic, out ITypeSymbol type) && type.GetMembers().OfType<IMethodSymbol>().Any(m => m.HasUpdateAttribute()))
                {
                    context.ReportSuppression(diagnostic, PartialRule);
                }
                else if (context.TryGetMethodSymbol(diagnostic, out IMethodSymbol method) && method.HasUpdateAttribute())
                {
                    context.ReportSuppression(diagnostic, UnusedRule);
                    context.ReportSuppression(diagnostic, NonReadOnlyStructAsReadOnlyReferenceRule);
                }
            }
        }
    }
}
