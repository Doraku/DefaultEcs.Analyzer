﻿using System.Collections.Immutable;
using System.Linq;
using DefaultEcs.Analyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class WorldComponentAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor InheritEntitySystemRule = new(
            "DEA0019",
            "The WorldComponent attribute should be used on a member of a type which inherit from AEntitySetSystem, AEntitySortedSetSystem or AEntityMultiMapSystem",
            "Remove the WorldComponent attribute from this member or change the inherited type of the current type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "The WorldComponent attribute should be used on a member of a type which inherit from AEntitySetSystem, AEntitySortedSetSystem or AEntityMultiMapSystem.");

        public static readonly DiagnosticDescriptor UpdateAttributeRule = new(
            "DEA0020",
            "The WorldComponent attribute should be used on a member of a type which has a method with a Update attribute",
            "Remove the WorldComponent attribute from this member or add an Update attribute on a method of its type",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "The WorldComponent attribute should be used on a member of a type which has a method with a Update attribute.");

        public static readonly DiagnosticDescriptor NoConstructorRule = new(
            "DEA0021",
            "The WorldComponent attribute should be used on a member of a type which has no constructor defined",
            "Remove the WorldComponent attribute from this member or remove its type constructor",
            DiagnosticCategory.Correctness,
            DiagnosticSeverity.Info,
            true,
            "The WorldComponent attribute should be used on a member of a type which has no constructor defined.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InheritEntitySystemRule, UpdateAttributeRule, NoConstructorRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field, SymbolKind.Property);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol.HasWorldComponentAttribute())
            {
                if (!context.Symbol.ContainingType.IsEntitySystem())
                {
                    context.ReportDiagnostic(Diagnostic.Create(InheritEntitySystemRule, context.Symbol.Locations[0]));
                }

                if (context.Symbol.ContainingType.GetMembers().OfType<IMethodSymbol>().All(m => !m.HasUpdateAttribute()))
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
