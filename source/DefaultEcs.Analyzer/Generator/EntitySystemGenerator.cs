using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefaultEcs.Analyzer.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DefaultEcs.Analyzer
{
    [Generator]
    public sealed class EntitySystemGenerator : ISourceGenerator
    {
        private static Compilation GenerateAttributes(GeneratorExecutionContext context)
        {
            const string attributesSource =
@"
using System;
using System.Runtime.CompilerServices;

namespace DefaultEcs.System
{
    /// <summary>
    /// Makes so that the decorated method will be called by the current system type when no Update method are overridden.
    /// </summary>
    [CompilerGenerated, AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}";

            context.AddSource("Attributes", attributesSource);

            return context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
                SourceText.From(attributesSource, Encoding.UTF8),
                (context.Compilation as CSharpCompilation)?.SyntaxTrees[0].Options as CSharpParseOptions));
        }

        public void Initialize(GeneratorInitializationContext context)
        { }

        public void Execute(GeneratorExecutionContext context)
        {
            static string GetName(ITypeSymbol type) => type.TypeKind is TypeKind.TypeParameter || (type.SpecialType > 0 && type.SpecialType <= SpecialType.System_String) ? type.ToString() : $"global::{type}";

            Compilation compilation = GenerateAttributes(context);
            int systemCount = 0;

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);

                StringBuilder code = new StringBuilder();
                foreach (IMethodSymbol method in tree
                    .GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<MethodDeclarationSyntax>()
                    .Select(m => semanticModel.GetDeclaredSymbol(m))
                    .Where(m => m.HasUpdateAttribute()
                        && !m.IsGenericMethod
                        && m.ContainingType.IsPartial()
                        && m.ContainingType.IsEntitySystem()
                        && m.ContainingType.GetMembers().OfType<IMethodSymbol>().Count(m => m.HasUpdateAttribute()) is 1
                        && !m.ContainingType.GetMembers().OfType<IMethodSymbol>().Any(m => m.IsEntitySystemUpdateOverride())
                        && !m.Parameters.Any(p => p.RefKind == RefKind.Out)))
                {
                    INamedTypeSymbol type = method.ContainingType;
                    code.Clear();

                    List<string> updateOverrideParameters = new();
                    List<string> parameters = new();
                    List<string> components = new();
                    HashSet<ITypeSymbol> parameterTypes = new(SymbolEqualityComparer.IncludeNullability);
                    bool canRemoveReflection = !type.GetMembers().OfType<IMethodSymbol>().Any(m => m.HasWithPredicateAttribute() && !m.IsStatic);

                    bool isBufferType = false;
                    if (type.IsAEntitySystem(out IList<ITypeSymbol> genericTypes))
                    {
                        updateOverrideParameters.Add($"{GetName(genericTypes[0])} state");
                    }
                    else if (type.IsAEntitiesSystem(out genericTypes))
                    {
                        updateOverrideParameters.Add($"{GetName(genericTypes[0])} state");
                        updateOverrideParameters.Add($"in {GetName(genericTypes[1])} key");

                        canRemoveReflection &= !type.IsIEqualityComparer(genericTypes[1]);
                    }
                    else if (type.IsAEntityBufferedSystem(out genericTypes))
                    {
                        updateOverrideParameters.Add($"{GetName(genericTypes[0])} state");
                        isBufferType = true;
                    }
                    else if (type.IsAEntitiesBufferedSystem(out genericTypes))
                    {
                        updateOverrideParameters.Add($"{GetName(genericTypes[0])} state");
                        updateOverrideParameters.Add($"in {GetName(genericTypes[1])} key");
                        isBufferType = true;

                        canRemoveReflection &= !type.IsIEqualityComparer(genericTypes[1]);
                    }

                    foreach (IParameterSymbol parameter in method.Parameters)
                    {
                        if (parameter.Type.IsEntity() && parameter.RefKind != RefKind.Ref)
                        {
                            parameters.Add("entity");
                        }
                        else if (SymbolEqualityComparer.Default.Equals(parameter.Type, genericTypes[0]) && parameter.RefKind == RefKind.None)
                        {
                            parameters.Add("state");
                        }
                        else if (genericTypes.Count > 1 && SymbolEqualityComparer.Default.Equals(parameter.Type, genericTypes[1]) && parameter.RefKind != RefKind.Ref)
                        {
                            parameters.Add("key");
                        }
                        else if (parameter.Type.IsComponents() && parameter.Type is INamedTypeSymbol componentType)
                        {
                            string name = $"components{components.Count}";

                            components.Add($"            {parameter.Type} {name} = World.GetComponents<global::{componentType.TypeArguments[0]}>();");
                            parameters.Add((parameter.RefKind == RefKind.Ref ? "ref " : string.Empty) + name);
                        }
                        else
                        {
                            string name = $"components{components.Count}";

                            string typeName = GetName(parameter.Type);
                            parameterTypes.Add(parameter.Type);

                            components.Add($"            Components<{typeName}> {name} = World.GetComponents<{typeName}>();");
                            parameters.Add($"{(parameter.RefKind == RefKind.Ref ? "ref " : string.Empty)}{name}[entity]");
                        }
                    }

                    List<INamedTypeSymbol> parentTypes = type.GetParentTypes().Skip(1).Reverse().ToList();

                    code.AppendLine("using System;");
                    code.AppendLine("using System.Collections.Generic;");
                    code.AppendLine("using System.Runtime.CompilerServices;");
                    code.AppendLine("using DefaultEcs;");
                    code.AppendLine("using DefaultEcs.System;");
                    code.AppendLine("using DefaultEcs.Threading;");
                    code.AppendLine();
                    code.Append("namespace ").AppendLine(type.GetNamespace());
                    code.AppendLine("{");

                    foreach (INamedTypeSymbol parentType in parentTypes)
                    {
                        code.Append("    ").Append(parentType.DeclaredAccessibility.ToCode()).Append(" partial ").Append(parentType.TypeKind.ToCode()).Append(' ').AppendLine(parentType.GetName());
                        code.AppendLine("    {");
                    }

                    code.Append("    [With(").Append(string.Join(", ", parameterTypes.Where(t => !t.HasTypeParameter()).Select(t => $"typeof({GetName(t)})"))).AppendLine(")]");
                    code.Append("    ").Append(type.DeclaredAccessibility.ToCode()).Append(" partial class ").AppendLine(type.GetName());
                    code.AppendLine("    {");

                    string firstParameter = "world";
                    if (canRemoveReflection)
                    {
                        firstParameter += type.HasDisabledAttribute() ? ".GetDisabledEntities()" : ".GetEntities()";

                        foreach (ITypeSymbol parameterType in parameterTypes)
                        {
                            firstParameter += $".With<{GetName(parameterType)}>()";
                        }

                        foreach (AttributeData attribute in type.GetComponentAttributes())
                        {
                            string Get(string methodName) => string.Concat(attribute.ConstructorArguments[0].Values.Select(v => v.Value).OfType<ITypeSymbol>().Select(t => $".{methodName}<{GetName(t)}>()"));

                            string GetEither(string methodName)
                            {
                                ITypeSymbol[] types = attribute.ConstructorArguments[0].Values.Select(v => v.Value).OfType<ITypeSymbol>().ToArray();

                                return types.Length > 0
                                    ? $".{methodName}Either<{GetName(types[0])}>(){string.Concat(types.Skip(1).Select(t => $".Or<{GetName(t)}>()"))}"
                                    : string.Empty;
                            }

                            firstParameter += attribute.AttributeClass.Name switch
                            {
                                "WithAttribute" => Get("With"),
                                "WithEitherAttribute" => GetEither("With"),
                                "WithoutAttribute" => Get("Without"),
                                "WithoutEitherAttribute" => GetEither("Without"),
                                "WhenAddedAttribute" => Get("WhenAdded"),
                                "WhenAddedEitherAttribute" => GetEither("WhenAdded"),
                                "WhenChangedAttribute" => Get("WhenChanged"),
                                "WhenChangedEitherAttribute" => GetEither("WhenChanged"),
                                "WhenRemovedAttribute" => Get("WhenRemoved"),
                                "WhenRemovedEitherAttribute" => GetEither("WhenRemoved"),
                                _ => string.Empty
                            };
                        }

                        foreach (IMethodSymbol predicate in type.GetMembers().OfType<IMethodSymbol>().Where(m => m.HasWithPredicateAttribute() && m.Parameters.Length == 1))
                        {
                            firstParameter += $".With<{GetName(predicate.Parameters[0].Type)}>({predicate.Name})";
                        }

                        firstParameter += genericTypes.Count is 1 ? ".AsSet()" : $".AsMultiMap<{GetName(genericTypes[1])}>()";
                    }

                    string constructorVisibility = type.Constructors.All(c => c.IsImplicitlyDeclared) ? (type.IsAbstract ? "protected" : "public") : "private";

                    if (isBufferType)
                    {
                        if (!type.Constructors.Any(c =>
                            c.Parameters.Length is 1
                            && c.Parameters[0].Type.IsWorld()))
                        {
                            code.AppendLine("        [CompilerGenerated]");
                            code.Append("        ").Append(constructorVisibility).Append(' ').Append(type.Name).AppendLine("(World world)");
                            code.Append("            : base(").Append(firstParameter).AppendLine(")");
                            code.AppendLine("        { }");
                            code.AppendLine();
                        }
                    }
                    else
                    {
                        if (!type.Constructors.Any(c =>
                            c.Parameters.Length is 3
                            && c.Parameters[0].Type.IsWorld()
                            && c.Parameters[1].Type.IsIParallelRunner()
                            && c.Parameters[2].Type.SpecialType is SpecialType.System_Int32))
                        {
                            code.AppendLine("        [CompilerGenerated]");
                            code.Append("        ").Append(constructorVisibility).Append(' ').Append(type.Name).AppendLine("(World world, IParallelRunner runner, int minEntityCountByRunnerIndex)");
                            code.Append("            : base(").Append(firstParameter).AppendLine(", runner, minEntityCountByRunnerIndex)");
                            code.AppendLine("        { }");
                            code.AppendLine();
                        }

                        if (!type.Constructors.Any(c =>
                            c.Parameters.Length is 2
                            && c.Parameters[0].Type.IsWorld()
                            && c.Parameters[1].Type.IsIParallelRunner()))
                        {
                            code.AppendLine("        [CompilerGenerated]");
                            code.Append("        ").Append(constructorVisibility).Append(' ').Append(type.Name).AppendLine("(World world, IParallelRunner runner)");
                            code.Append("            : base(").Append(firstParameter).AppendLine(", runner)");
                            code.AppendLine("        { }");
                            code.AppendLine();
                        }

                        if (!type.Constructors.Any(c =>
                            c.Parameters.Length is 1
                            && c.Parameters[0].Type.IsWorld()))
                        {
                            code.AppendLine("        [CompilerGenerated]");
                            code.Append("        ").Append(constructorVisibility).Append(' ').Append(type.Name).AppendLine("(World world)");
                            code.Append("            : base(").Append(firstParameter).AppendLine(")");
                            code.AppendLine("        { }");
                            code.AppendLine();
                        }
                    }

                    code.AppendLine("        [CompilerGenerated]");
                    code.Append("        protected override void Update(").Append(string.Join(", ", updateOverrideParameters)).AppendLine(", ReadOnlySpan<Entity> entities)");
                    code.AppendLine("        {");

                    foreach (string component in components)
                    {
                        code.AppendLine(component);
                    }

                    code.AppendLine("            foreach (ref readonly Entity entity in entities)");
                    code.AppendLine("            {");
                    code.Append("                ").Append(method.Name).Append('(').Append(string.Join(", ", parameters)).AppendLine(");");
                    code.AppendLine("            }");
                    code.AppendLine("        }");
                    code.AppendLine("    }");

                    for (int i = 0; i < parentTypes.Count; ++i)
                    {
                        code.AppendLine("    }");
                    }

                    code.AppendLine("}");

                    context.AddSource($"System{++systemCount}", code.ToString());
                }
            }
        }
    }
}
