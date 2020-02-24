using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SubscribeAttributeAnalyser : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "DEA0001",
            "SubscribeAttribute used on an invalid method",
            "Remove SubscribeAttribute from the '{0}' method or change the method signature.",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Error,
            true,
            "SubscribeAttribute should only be used on method with the MessageHandler signature.");

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
                && method.GetAttributes().Any(a => a.ToString() == "DefaultEcs.SubscribeAttribute")
                && (!method.ReturnsVoid || method.Parameters.Length != 1 || method.Parameters[0].RefKind != RefKind.In))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
            }
        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SubscribeAttributeSuppressor : DiagnosticSuppressor
    {
        public static readonly SuppressionDescriptor Rule = new SuppressionDescriptor(
            "DES0001",
            "IDE0051",
            "Private member is used by reflection.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(Rule);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics.Where(d => d.Id == Rule.SuppressedDiagnosticId))
            {
                if (diagnostic.Location.SourceTree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan) is MethodDeclarationSyntax methodDeclaration
                    && context.GetSemanticModel(diagnostic.Location.SourceTree).GetDeclaredSymbol(methodDeclaration) is IMethodSymbol method
                    && method.GetAttributes().Any(a => a.ToString() == "DefaultEcs.SubscribeAttribute"))
                {
                    context.ReportSuppression(Suppression.Create(Rule, diagnostic));
                }
            }
        }
    }
}
