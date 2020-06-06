using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class SuppressionAnalysisContextExtension
    {
        public static void ReportSuppression(this SuppressionAnalysisContext context, SuppressionDescriptor descriptor, Diagnostic diagnostic)
        {
            if (diagnostic.Id == descriptor.SuppressedDiagnosticId)
            {
                context.ReportSuppression(Suppression.Create(descriptor, diagnostic));
            }
        }
    }
}
