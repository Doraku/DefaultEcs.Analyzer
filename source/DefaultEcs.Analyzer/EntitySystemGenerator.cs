
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer
{
    [Generator]
    public sealed class EntitySystemGenerator : ISourceGenerator
    {
        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                //if (syntaxNode is MethodDeclarationSyntax method
                //    && method.Parent is TypeDeclarationSyntax type)
                //{

                //}
            }
        }

        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            const string attributesSource =
@"
using System;

namespace DefaultEcs.System
{
    /// <summary>
    /// Makes so that the decorated method will be called by the current system type when no Update method are overridden.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UpdateAttribute : Attribute
    { }
}";

            context.AddSource("Attributes", attributesSource);

            if (context.SyntaxReceiver is not SyntaxReceiver)
                return;
        }
    }
}
