﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EntitySystemAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableHashSet<string> _entityChangeMethods = ImmutableHashSet.Create(
            "Set",
            "SetSameAs",
            "Remove",
            "Enable",
            "Disable",
            "NotifyChanged",
            "Dispose");

        public static readonly DiagnosticDescriptor NoEntityModificationRule = new DiagnosticDescriptor(
            "DEA0005",
            "Entity modification method '{0}' used inside the Update method of AEntitySystem or AEntitiesSystem",
            "Use an EntityCommandRecorder or change the system to an AEntityBufferedSystem or AEntitiesBufferedSystem",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Warning,
            true,
            "Entity modification methods are not thread safe and should not be used inside the Update method of AEntitySystem or AEntitiesSystem.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoEntityModificationRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterOperationAction(AnalyzeOperation, OperationKind.MethodBody);
        }

        private static void AnalyzeOperation(OperationAnalysisContext context)
        {
            if (context.ContainingSymbol is IMethodSymbol method
                && (method.ContainingType.IsAEntitySystem(out IList<ITypeSymbol> genericTypes) || method.ContainingType.IsAEntitiesSystem(out genericTypes))
                && (method.HasUpdateAttribute() || method.IsEntitySystemUpdateOverride(genericTypes)))
            {
                foreach (InvocationExpressionSyntax invocation in method.DeclaringSyntaxReferences.SelectMany(r => r.GetSyntax().DescendantNodes().OfType<InvocationExpressionSyntax>()))
                {
                    IMethodSymbol invokedMethod = context.Operation.SemanticModel.GetSymbolInfo(invocation.Expression).As<IMethodSymbol>();
                    if (invokedMethod?.ContainingType.IsEntity() is true && _entityChangeMethods.Contains(invokedMethod.Name))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(NoEntityModificationRule, invocation.GetLocation(), invokedMethod.Name));
                    }
                }
            }
        }
    }
}