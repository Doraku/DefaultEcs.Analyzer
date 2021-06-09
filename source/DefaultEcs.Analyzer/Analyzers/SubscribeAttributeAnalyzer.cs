using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SubscribeAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor CorrectSignatureRule = new(
            "DEA0001",
            "SubscribeAttribute used on an invalid method",
            "Remove SubscribeAttribute from the '{0}' method or change the method signature",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Error,
            true,
            "SubscribeAttribute should only be used on method with the MessageHandler signature.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CorrectSignatureRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is IMethodSymbol method
                && method.HasSubscribeAttribute()
                && (!method.ReturnsVoid || method.Parameters.Length != 1 || method.Parameters[0].RefKind != RefKind.In))
            {
                context.ReportDiagnostic(Diagnostic.Create(CorrectSignatureRule, method.Locations[0], method.Name));
            }
        }
    }
}
