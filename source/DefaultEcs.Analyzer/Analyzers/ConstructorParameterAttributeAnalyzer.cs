using System.Collections.Immutable;
using System.Linq;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConstructorParameterAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor InheritEntitySystemRule = new DiagnosticDescriptor(
            "DEA0013",
            "The ConstructorParameter attribute should be used on a member of a type which inherit from AEntitySystem, AEntitiesSystem, AEntityBufferedSystem or AEntitiesBufferedSystem",
            "Remove the ConstructorParameter attribute from this member or change the inherited type of the current type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Warning,
            true,
            "The ConstructorParameter attribute should be used on a member of a type which inherit from AEntitySystem, AEntitiesSystem, AEntityBufferedSystem or AEntitiesBufferedSystem.");

        public static readonly DiagnosticDescriptor HasUpdateAttributeSystemRule = new DiagnosticDescriptor(
            "DEA0014",
            "The ConstructorParameter attribute should be used on a member of a type which has a method with a Update attribute",
            "Remove the ConstructorParameter attribute from this member or add an Update attribute on a method of its type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Warning,
            true,
            "The ConstructorParameter attribute should be used on a member of a type which has a method with a Update attribute.");

        public static readonly DiagnosticDescriptor HasNoConstructorRule = new DiagnosticDescriptor(
            "DEA0015",
            "The ConstructorParameter attribute should be used on a member of a type which has no constructor defined",
            "Remove the ConstructorParameter attribute from this member or remove its type constructor",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Warning,
            true,
            "The ConstructorParameter attribute should be used on a member of a type which has no constructor defined.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InheritEntitySystemRule, HasUpdateAttributeSystemRule, HasNoConstructorRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field, SymbolKind.Property);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol.HasConstructorParameterAttribute())
            {
                if (!context.Symbol.ContainingType.IsEntitySystem())
                {
                    context.ReportDiagnostic(Diagnostic.Create(InheritEntitySystemRule, context.Symbol.Locations[0]));
                }

                if (context.Symbol.ContainingType.GetMembers().OfType<IMethodSymbol>().All(m => !m.HasUpdateAttribute()))
                {
                    context.ReportDiagnostic(Diagnostic.Create(HasUpdateAttributeSystemRule, context.Symbol.Locations[0]));
                }

                if (context.Symbol.ContainingType.Constructors.Any(c => !c.IsImplicitlyDeclared))
                {
                    context.ReportDiagnostic(Diagnostic.Create(HasNoConstructorRule, context.Symbol.Locations[0]));
                }
            }
        }
    }
}
