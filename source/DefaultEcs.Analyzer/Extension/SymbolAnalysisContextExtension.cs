using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class SymbolAnalysisContextExtension
    {
        public static void ReportDiagnostic(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location location, params object[] messageArgs)
            => context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(descriptor, location, messageArgs));
    }
}
