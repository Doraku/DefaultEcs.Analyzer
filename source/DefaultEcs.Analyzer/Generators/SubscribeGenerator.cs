//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;

//namespace DefaultEcs.Analyzer.Generators
//{
//    [Generator]
//    public sealed class SubscribeGenerator : ISourceGenerator
//    {
//        public void Initialize(GeneratorInitializationContext context)
//        { }

//        public void Execute(GeneratorExecutionContext context)
//        {
//            StringBuilder extensionsCode = new StringBuilder();
//            HashSet<ITypeSymbol> generatedTypes = new(SymbolEqualityComparer.IncludeNullability);

//            foreach (SyntaxTree tree in context.Compilation.SyntaxTrees)
//            {
//                SemanticModel semanticModel = context.Compilation.GetSemanticModel(tree);

//                StringBuilder code = new StringBuilder();
//                foreach (InvocationExpressionSyntax invocation in tree
//                    .GetRoot()
//                    .DescendantNodesAndSelf()
//                    .OfType<InvocationExpressionSyntax>())
//                {
//                }
//            }
//        }
//    }
//}
