using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AEntitySystemAnalyser : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "DEA0005",
            "Entity modification method '{0}' used inside the Update method of AEntitySystem",
            "Use an EntityCommandRecorder or change the system to an AEntityBufferedSystem.",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Warning,
            true,
            "Entity modification methods are not thread safe and should not be used inside the Update method of AEntitySystem.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is IMethodSymbol method
                && method.IsAEntitySystemOverride())
            {
                foreach ((InvocationExpressionSyntax invocation, IMethodSymbol entityMethod) in method.GetEntityInvocations(context))
                {
                    if (entityMethod.Name != "Get")
                    {
                        context.ReportDiagnostic(Rule, invocation.GetLocation(), entityMethod.Name);
                    }
                }
            }
        }
    }
}
