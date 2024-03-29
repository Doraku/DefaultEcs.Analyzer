﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DefaultEcs.Analyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultEcs.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EntitySystemAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableHashSet<string> _worldChangeMethods = ImmutableHashSet.Create(
            "CreateEntity",
            "SetMaxCapacity",
            "Set",
            "Remove",
            "Optimize",
            "TrimExcess",
            "Dispose");

        private static readonly ImmutableHashSet<string> _entityChangeMethods = ImmutableHashSet.Create(
            "Set",
            "SetSameAs",
            "SetSameAsWorld",
            "Remove",
            "Enable",
            "Disable",
            "NotifyChanged",
            "CopyTo",
            "Dispose");

        public static readonly DiagnosticDescriptor NoWorldModificationRule = new(
            "DEA0022",
            "World modification method '{0}' used inside the Update method of AEntitySetSystem, AEntitySortedSetSystem or AEntityMultiMapSystem which does not use buffer",
            "Use an EntityCommandRecorder or change constructor to use buffer",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Warning,
            true,
            "World modification methods are not thread safe and should not be used inside the Update method of AEntitySetSystem, AEntitySortedSetSystem or AEntityMultiMapSystem.");

        public static readonly DiagnosticDescriptor NoEntityModificationRule = new(
            "DEA0005",
            "Entity modification method '{0}' used inside the Update method of AEntitySetSystem, AEntitySortedSetSystem or AEntityMultiMapSystem which does not use buffer",
            "Use an EntityCommandRecorder or change constructor to use buffer",
            DiagnosticCategory.RuntimeError,
            DiagnosticSeverity.Warning,
            true,
            "Entity modification methods are not thread safe and should not be used inside the Update method of AEntitySetSystem, AEntitySortedSetSystem or AEntityMultiMapSystem.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoWorldModificationRule, NoEntityModificationRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterOperationAction(AnalyzeOperation, OperationKind.MethodBody);
        }

        private static void AnalyzeOperation(OperationAnalysisContext context)
        {
            static bool CheckUseBuffer(ConstructorDeclarationSyntax constructor) => constructor.Initializer?.ArgumentList.Arguments.Count is 2
                && constructor.Initializer?.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax literal
                && literal.Token.Value is true;

            if (context.ContainingSymbol is IMethodSymbol method
                && method.ContainingType.IsEntitySystem()
                && ((method.HasUpdateAttribute() && !method.HasUseBufferAttribute()) || method.IsEntitySystemUpdateOverride())
                && !method.ContainingType.Constructors.Any(c => c.Locations.Select(l => l.SourceTree.GetRoot().FindNode(l.SourceSpan)).OfType<ConstructorDeclarationSyntax>().Any(CheckUseBuffer)))
            {
                foreach (InvocationExpressionSyntax invocation in method.DeclaringSyntaxReferences.SelectMany(r => r.GetSyntax().DescendantNodes().OfType<InvocationExpressionSyntax>()))
                {
                    IMethodSymbol invokedMethod = context.Operation.SemanticModel.GetSymbolInfo(invocation.Expression).As<IMethodSymbol>();

                    if (invokedMethod?.ContainingType.IsWorld() is true && _worldChangeMethods.Contains(invokedMethod.Name) && !method.ContainingType.IsAEntitySortedSetSystem(out IList<ITypeSymbol> _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(NoWorldModificationRule, invocation.GetLocation(), invokedMethod.Name));
                    }

                    if (invokedMethod?.ContainingType.IsEntity() is true && _entityChangeMethods.Contains(invokedMethod.Name))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(NoEntityModificationRule, invocation.GetLocation(), invokedMethod.Name));
                    }
                }
            }
        }
    }
}
