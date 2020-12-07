using System.Collections.Immutable;
using System.Linq;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EntityCommandRecorderAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor InheritNonBufferedEntitySystemRule = new DiagnosticDescriptor(
            "DEA0016",
            "The EntityCommandRecorder attribute should be used on a member of a type which inherit from AEntitySystem or AEntitiesSystem",
            "Remove the EntityCommandRecorder attribute from this member or change the inherited type of the current type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Warning,
            true,
            "The EntityCommandRecorder attribute should be used on a member of a type which inherit from AEntitySystem or AEntitiesSystem.");

        public static readonly DiagnosticDescriptor SingleEntityCommandRecorderRule = new DiagnosticDescriptor(
            "DEA0017",
            "Only one member can be decorated with the EntityCommandRecorder attribute in a given type",
            "Remove extra EntityCommandRecorder attribute",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Error,
            true,
            "Only one member can be decorated with the EntityCommandRecorder attribute in a given type.");

        public static readonly DiagnosticDescriptor UpdateAttributeRule = new DiagnosticDescriptor(
            "DEA0018",
            "The EntityCommandRecorder attribute should be used on a member of a type which has a method with a Update attribute",
            "Remove the EntityCommandRecorder attribute from this member or add an Update attribute on a method of its type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Warning,
            true,
            "The EntityCommandRecorder attribute should be used on a member of a type which has a method with a Update attribute.");

        public static readonly DiagnosticDescriptor CorrectTypeRule = new DiagnosticDescriptor(
            "DEA0019",
            "The EntityCommandRecorder attribute should be used on a field or property of type EntityCommandRecorder",
            "Remove the EntityCommandRecorder attribute from this member or change its type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Error,
            true,
            "The EntityCommandRecorder attribute should be used on a field or property of type EntityCommandRecorder.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InheritNonBufferedEntitySystemRule, SingleEntityCommandRecorderRule, UpdateAttributeRule, CorrectTypeRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field, SymbolKind.Property);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol.HasEntityCommandRecorderAttribute())
            {
                if (!context.Symbol.ContainingType.IsAEntitySystem(out var _)
                    && !context.Symbol.ContainingType.IsAEntitiesSystem(out var _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(InheritNonBufferedEntitySystemRule, context.Symbol.Locations[0]));
                }

                if (context.Symbol.ContainingType.GetMembers().OfType<IMethodSymbol>().All(m => !m.HasUpdateAttribute()))
                {
                    context.ReportDiagnostic(Diagnostic.Create(UpdateAttributeRule, context.Symbol.Locations[0]));
                }

                if (context.Symbol.ContainingType.GetMembers().Count(m => m.HasEntityCommandRecorderAttribute()) > 1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(SingleEntityCommandRecorderRule, context.Symbol.Locations[0]));
                }

                if ((context.Symbol switch { IFieldSymbol field => field.Type, IPropertySymbol property => property.Type, _ => null })?.IsEntityCommandRecorder() != true)
                {
                    context.ReportDiagnostic(Diagnostic.Create(CorrectTypeRule, context.Symbol.Locations[0]));
                }
            }
        }
    }
}
