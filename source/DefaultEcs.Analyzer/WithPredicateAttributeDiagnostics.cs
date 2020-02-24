using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WithPredicateAttributeAnalyser : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "DEA0002",
            "WithPredicateAttribute used on an invalid method",
            "Remove WithPredicateAttribute from the '{0}' method or change the method signature.",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Error,
            true,
            "WithPredicateAttribute should only be used on method with the ComponentPredicate signature.");

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
                && method.GetAttributes().Any(a => a.ToString() == "DefaultEcs.System.WithPredicateAttribute")
                && (method.ReturnType.SpecialType != SpecialType.System_Boolean || method.Parameters.Length != 1 || method.Parameters[0].RefKind != RefKind.In))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
            }
        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WithPredicateAttributeSuppressor : DiagnosticSuppressor
    {
        public static readonly SuppressionDescriptor Rule = new SuppressionDescriptor(
            "DES0002",
            "IDE0051",
            "Private member is used by reflection.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(Rule);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics.Where(d => d.Id == Rule.SuppressedDiagnosticId))
            {
                if (diagnostic.Location.SourceTree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan) is MethodDeclarationSyntax methodDeclaration
                    && context.GetSemanticModel(diagnostic.Location.SourceTree).GetDeclaredSymbol(methodDeclaration) is IMethodSymbol method
                    && method.GetAttributes().Any(a => a.ToString() == "DefaultEcs.System.WithPredicateAttribute"))
                {
                    context.ReportSuppression(Suppression.Create(Rule, diagnostic));
                }
            }
        }
    }
}
