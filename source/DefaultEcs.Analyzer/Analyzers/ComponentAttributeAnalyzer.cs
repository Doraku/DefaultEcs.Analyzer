using System.Collections.Immutable;
using DefaultEcs.Analyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ComponentAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor CorrectSignatureRule = new(
            "DEA0002",
            "WithPredicateAttribute used on an invalid method",
            "Remove WithPredicateAttribute from the '{0}' method or change the method signature",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Error,
            true,
            "WithPredicateAttribute should only be used on method with the ComponentPredicate signature.");

        public static readonly DiagnosticDescriptor ContainingTypeInheritEntitySystemRule = new(
            "DEA0003",
            "WithPredicateAttribute used on a method which is not a member of AEntitySetSystem or AEntityMultiMapSystem",
            "Remove WithPredicateAttribute from the '{0}' method",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "WithPredicateAttribute should only be used on method member of AEntitySetSystem or AEntityMultiMapSystem.");

        public static readonly DiagnosticDescriptor InheritEntitySystemRule = new(
            "DEA0004",
            "Component attribute used on a type which is not derived from AEntitySetSystem or AEntityMultiMapSystem",
            "Remove '{1}' from the '{0}' type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "Component attribute should only be used on type which is derived from AEntitySetSystem or AEntityMultiMapSystem.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CorrectSignatureRule, ContainingTypeInheritEntitySystemRule, InheritEntitySystemRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is IMethodSymbol method && method.HasWithPredicateAttribute())
            {
                if (!method.ContainingType.IsEntitySystem())
                {
                    context.ReportDiagnostic(Diagnostic.Create(ContainingTypeInheritEntitySystemRule, method.Locations[0], method.Name));
                }
                else if (method.ReturnType.SpecialType != SpecialType.System_Boolean || method.Parameters.Length != 1 || method.Parameters[0].RefKind != RefKind.In)
                {
                    context.ReportDiagnostic(Diagnostic.Create(CorrectSignatureRule, method.Locations[0], method.Name));
                }
            }

            if (context.Symbol is INamedTypeSymbol type
                && type.HasComponentAttribute()
                && !type.IsEntitySystem())
            {
                foreach (AttributeData attribute in type.GetComponentAttributes())
                {
                    context.ReportDiagnostic(Diagnostic.Create(InheritEntitySystemRule, type.Locations[0], type.Name, attribute.AttributeClass.Name));
                }
            }
        }
    }
}
