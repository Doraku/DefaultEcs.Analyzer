using System.Collections.Immutable;
using System.Linq;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UpdateAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor InheritEntitySystemRule = new DiagnosticDescriptor(
            "DEA0006",
            "The Update attribute should be used on a method of a type which inherit from AEntitySystem, AEntitiesSystem, AEntityBufferedSystem or AEntitiesBufferedSystem",
            "Remove the Update attribute from this method or change the inherited type of the current type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Warning,
            true,
            "The Update attribute should be used on a method of a type which inherit from AEntitySystem, AEntitiesSystem, AEntityBufferedSystem or AEntitiesBufferedSystem.");

        public static readonly DiagnosticDescriptor SingleUpdateAttributeRule = new DiagnosticDescriptor(
            "DEA0007",
            "Only one method can be decorated with the Update attribute in a given type",
            "Remove extra Update attribute",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Error,
            true,
            "Only one method can be decorated with the Update attribute in a given type.");

        public static readonly DiagnosticDescriptor NoUpdateOverrideRule = new DiagnosticDescriptor(
            "DEA0008",
            "The Update attribute can't be used when an override of the Update method is already present",
            "Remove either the Update attribute or the method Update override",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Error,
            true,
            "The Update attribute can't be used when an override of the Update method is already present.");

        public static readonly DiagnosticDescriptor NoOutParameterRule = new DiagnosticDescriptor(
            "DEA0009",
            "No out parameter can be present in the method decorated by the Update attribute",
            "Remove the Update attribute or change the parameter modifier",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Error,
            true,
            "No out parameter can be present in the method decorated by the Update attribute.");

        public static readonly DiagnosticDescriptor PartialTypeRule = new DiagnosticDescriptor(
            "DEA0010",
            "The type containing the method decorated by the Update attribute should be partial",
            "Mark the parent type as partial",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Error,
            true,
            "The type containing the method decorated by the Update attribute should be partial.");

        public static readonly DiagnosticDescriptor VoidReturnRule = new DiagnosticDescriptor(
            "DEA0011",
            "The method decorated by the Update attribute should return void",
            "Change the return type of the method to void",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Warning,
            true,
            "The method decorated by the Update attribute should return void.");

        public static readonly DiagnosticDescriptor NoGenericRule = new DiagnosticDescriptor(
            "DEA0012",
            "The method decorated by the Update attribute should not be generic",
            "Remove the genericity of the method or the Update attribute",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Error,
            true,
            "he method decorated by the Update attribute should not be generic.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InheritEntitySystemRule, SingleUpdateAttributeRule, NoUpdateOverrideRule, NoOutParameterRule, PartialTypeRule, VoidReturnRule, NoGenericRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is IMethodSymbol method
                && method.HasUpdateAttribute())
            {
                foreach (INamedTypeSymbol type in method.ContainingType.GetParentTypes().Where(t => !t.IsPartial()))
                {
                    context.ReportDiagnostic(Diagnostic.Create(PartialTypeRule, type.Locations[0]));
                }

                if (!method.ContainingType.IsEntitySystem())
                {
                    context.ReportDiagnostic(Diagnostic.Create(InheritEntitySystemRule, method.Locations[0]));
                }

                if (method.ContainingType.GetMembers().OfType<IMethodSymbol>().Any(m => m.IsEntitySystemUpdateOverride() && !m.HasCompilerGeneratedAttribute()))
                {
                    context.ReportDiagnostic(Diagnostic.Create(NoUpdateOverrideRule, method.Locations[0]));
                }

                if (method.ContainingType.GetMembers().OfType<IMethodSymbol>().Count(m => m.HasUpdateAttribute()) > 1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(SingleUpdateAttributeRule, method.Locations[0]));
                }

                if (method.Parameters.Any(p => p.RefKind == RefKind.Out))
                {
                    context.ReportDiagnostic(Diagnostic.Create(NoOutParameterRule, method.Locations[0]));
                }

                if (!method.ReturnsVoid)
                {
                    context.ReportDiagnostic(Diagnostic.Create(VoidReturnRule, method.Locations[0]));
                }

                if (method.IsGenericMethod)
                {
                    context.ReportDiagnostic(Diagnostic.Create(NoGenericRule, method.Locations[0]));
                }
            }
        }
    }
}
