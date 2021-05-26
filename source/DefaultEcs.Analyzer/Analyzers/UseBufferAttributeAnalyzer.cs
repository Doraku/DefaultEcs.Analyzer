using System.Collections.Immutable;
using System.Linq;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseBufferAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor UpdateAttributeRule = new(
            "DEA0017",
            "UseBuffer attribute should only be used on method decorated with an Update attribute",
            "Remove extra UseBuffer attribute",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "UseBuffer attribute should only be used on method decorated with an Update attribute.");

        public static readonly DiagnosticDescriptor NoConstructorRule = new(
            "DEA0018",
            "The UseBuffer attribute should be used on a method of a type which has no constructor defined",
            "Remove extra UseBuffer attribute",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "The UseBuffer attribute should be used on a method of a type which has no constructor defined.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            UpdateAttributeRule,
            NoConstructorRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
        }

        private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol.HasUseBufferAttribute())
            {
                if (!context.Symbol.HasUpdateAttribute())
                {
                    context.ReportDiagnostic(Diagnostic.Create(UpdateAttributeRule, context.Symbol.Locations[0]));
                }

                if (context.Symbol.ContainingType.Constructors.Any(c => !c.IsImplicitlyDeclared && !c.HasCompilerGeneratedAttribute()))
                {
                    context.ReportDiagnostic(Diagnostic.Create(NoConstructorRule, context.Symbol.Locations[0]));
                }
            }
        }
    }
}
