using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DefaultEcs.Analyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DefaultEcs.Analyzer.Generators
{
    [Generator]
    public sealed class SystemGenerator : ISourceGenerator
    {
        private static Compilation GenerateAttributes(GeneratorExecutionContext context)
        {
            const string helpersSource =
@"using System;
using System.Runtime.CompilerServices;

namespace DefaultEcs.System
{
    /// <summary>
    /// Makes so that the decorated method will be called by the current system type when no Update method are overridden.
    /// </summary>
    [CompilerGenerated, AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    /// <summary>
    /// Makes so that the entities will be copied into a buffer before processing.
    /// </summary>
    [CompilerGenerated, AttributeUsage(AttributeTargets.Method)]
    internal sealed class UseBufferAttribute : Attribute
    { }

    /// <summary>
    /// Used on a field or property that need to be set in the generated constructor.
    /// </summary>
    [CompilerGenerated, AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class ConstructorParameterAttribute : Attribute
    { }

    /// <summary>
    /// Used on a field or property that need to be set in the generated constructor from a World component.
    /// </summary>
    [CompilerGenerated, AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class WorldComponentAttribute : Attribute
    { }

    /// <summary>
    /// Used on a parameter of a method decorated by the UpdateAttribute to state that the rule generated for the parameter type is WhenAdded.
    /// </summary>
    [CompilerGenerated, AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class AddedAttribute : Attribute
    { }

    /// <summary>
    /// Used on a parameter of a method decorated by the UpdateAttribute to state that the rule generated for the parameter type is WhenChanged.
    /// </summary>
    [CompilerGenerated, AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class ChangedAttribute : Attribute
    { }
}
";

            context.AddSource("Attributes", helpersSource);

            return context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
                SourceText.From(helpersSource, Encoding.UTF8),
                (context.Compilation as CSharpCompilation)?.SyntaxTrees[0].Options as CSharpParseOptions));
        }

        private static string GetName(ITypeSymbol type) => type.TypeKind is TypeKind.TypeParameter || (type.SpecialType > 0 && type.SpecialType <= SpecialType.System_String) ? type.ToString() : $"global::{type}";

        private static void WriteFactory(
            StringBuilder code,
            INamedTypeSymbol type,
            string containerType,
            ITypeSymbol componentType,
            IEnumerable<ITypeSymbol> withTypes,
            IEnumerable<ITypeSymbol> addedTypes,
            IEnumerable<ITypeSymbol> changedTypes)
        {
            void WriteRules(string indentation, string name, IEnumerable<ITypeSymbol> types)
            {
                foreach (ITypeSymbol t in types)
                {
                    code.Append(indentation).Append('.').Append(name).Append('<').Append(GetName(t)).AppendLine(">()");
                }
            }

            code.AppendLine("        [CompilerGenerated]")
                .Append("        ").Append("private static ").Append("Entity").Append(containerType).Append(componentType is null ? string.Empty : $"<{GetName(componentType)}>").AppendLine(" CreateEntityContainer(object sender, World world)")
                .AppendLine("        {")
                .AppendLine("            var query = world")
                .Append("                ").AppendLine(type.HasDisabledAttribute() ? ".GetDisabledEntities()" : ".GetEntities()");

            WriteRules("                ", "With", withTypes);
            WriteRules("                ", "WhenAdded", addedTypes);
            WriteRules("                ", "WhenChanged", changedTypes);

            foreach (AttributeData attribute in type.GetComponentAttributes())
            {
                void Write(string methodName) => WriteRules("                ", methodName, attribute.ConstructorArguments[0].Values.Select(v => v.Value).OfType<ITypeSymbol>());

                void WriteEither(string methodName)
                {
                    ITypeSymbol[] types = attribute.ConstructorArguments[0].Values.Select(v => v.Value).OfType<ITypeSymbol>().ToArray();

                    WriteRules("                ", $"{methodName}Either", types.Take(1));
                    WriteRules("                    ", "Or", types.Skip(1));
                }

                switch (attribute.AttributeClass.Name)
                {
                    case "WithAttribute": Write("With"); break;
                    case "WithEitherAttribute": WriteEither("With"); break;
                    case "WithoutAttribute": Write("Without"); break;
                    case "WithoutEitherAttribute": WriteEither("Without"); break;
                    case "WhenAddedAttribute": Write("WhenAdded"); break;
                    case "WhenAddedEitherAttribute": WriteEither("WhenAdded"); break;
                    case "WhenChangedAttribute": Write("WhenChanged"); break;
                    case "WhenChangedEitherAttribute": WriteEither("WhenChanged"); break;
                    case "WhenRemovedAttribute": Write("WhenRemoved"); break;
                    case "WhenRemovedEitherAttribute": WriteEither("WhenRemoved"); break;
                }
            }

            foreach (IMethodSymbol predicate in type.GetMembers().OfType<IMethodSymbol>().Where(m => m.HasWithPredicateAttribute() && m.Parameters.Length == 1))
            {
                code.Append("                .With<").Append(GetName(predicate.Parameters[0].Type)).Append(">(").Append(predicate.IsStatic ? string.Empty : $"(({type.Name})sender).").Append(predicate.Name).AppendLine(")");
            }

            code.Length -= Environment.NewLine.Length;
            code.AppendLine(";");

            switch (containerType)
            {
                case "Set":
                    code.AppendLine("            return query.AsSet();");
                    break;

                case "SortedSet":
                    code.Append("            return sender is IComparer<").Append(GetName(componentType)).Append("> comparer ? query.AsSortedSet<").Append(GetName(componentType)).Append(">(comparer) : query.AsSortedSet<").Append(GetName(componentType)).AppendLine(">();");
                    break;

                case "MultiMap":
                    code.Append("            return sender is IEqualityComparer<").Append(GetName(componentType)).Append("> comparer ? query.AsMultiMap<").Append(GetName(componentType)).Append(">(comparer) : query.AsMultiMap<").Append(GetName(componentType)).AppendLine(">();");
                    break;
            }
            code.AppendLine("        }")
                .AppendLine();
        }

        private static void WriteConstructor(StringBuilder code, INamedTypeSymbol type, string parameters, string baseParameters)
        {
            string constructorVisibility = type.Constructors.All(c => c.IsImplicitlyDeclared) ? (type.IsAbstract ? "protected" : "public") : "private";

            List<(string type, string name, string parameterName)> extraParameters = new();
            List<(string type, string name)> worldComponents = new();

            void AddConstructorParameter(ITypeSymbol t, string name)
            {
                string parameterName = name.TrimStart('_');
                parameterName = $"{char.ToLower(parameterName[0], CultureInfo.InvariantCulture)}{parameterName.Substring(1)}";
                extraParameters.Add((GetName(t), name != parameterName ? name : $"this.{name}", parameterName));
            }

            void AddWorldComponent(ITypeSymbol t, string name) => worldComponents.Add((GetName(t), name));

            foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>().Where(f => !f.IsStatic && !f.IsConst))
            {
                if (field.HasConstructorParameterAttribute())
                {
                    AddConstructorParameter(field.Type, field.Name);
                }

                if (field.HasWorldComponentAttribute())
                {
                    AddWorldComponent(field.Type, field.Name);
                }
            }

            foreach (IPropertySymbol property in type.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic))
            {
                if (property.HasConstructorParameterAttribute())
                {
                    AddConstructorParameter(property.Type, property.Name);
                }

                if (property.HasWorldComponentAttribute())
                {
                    AddWorldComponent(property.Type, property.Name);
                }
            }

            code.AppendLine("        [CompilerGenerated]")
                .Append("        ").Append(constructorVisibility).Append(' ').Append(type.Name).Append('(').Append(parameters).Append(string.Concat(extraParameters.Select(p => $", {p.type} {p.parameterName}").Distinct())).AppendLine(")")
                .Append("            : base(").Append(baseParameters).AppendLine(")");
            if (extraParameters.Count is 0 && worldComponents.Count is 0)
            {
                code.AppendLine("        { }");
            }
            else
            {
                code.AppendLine("        {");
                foreach ((string _, string memberName, string parameterName) in extraParameters)
                {
                    code.Append("            ").Append(memberName).Append(" = ").Append(parameterName).AppendLine(";");
                }
                foreach ((string memberType, string memberName) in worldComponents)
                {
                    code.Append("            ").Append(memberName).Append(" = world.Has<").Append(memberType).Append(">() ? world.Get<").Append(memberType)
                        .Append(">() : throw new ArgumentException(\"Underlying World does not have a component of type ").Append(memberType).AppendLine("\", \"world\");");
                }
                code.AppendLine("        }");
            }
            code.AppendLine();
        }

        public void Initialize(GeneratorInitializationContext context)
        { }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.DisableDefaultEcsSystemGenerator", out string disableDefaultEcsSystemGenerator)
                && disableDefaultEcsSystemGenerator.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Compilation compilation = GenerateAttributes(context);
            int systemCount = 0;

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);

                StringBuilder code = new();
                foreach (IMethodSymbol method in tree
                    .GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<MethodDeclarationSyntax>()
                    .Select(m => semanticModel.GetDeclaredSymbol(m))
                    .Where(m => m.HasUpdateAttribute()
                        && !m.IsGenericMethod
                        && m.ContainingType.GetParentTypes().All(t => t.IsPartial())
                        && m.ContainingType.IsEntitySystem()
                        && m.ContainingType.GetMembers().OfType<IMethodSymbol>().Count(m => m.HasUpdateAttribute()) is 1
                        && !m.ContainingType.GetMembers().OfType<IMethodSymbol>().Any(m => m.IsEntitySystemUpdateOverride())
                        && !m.Parameters.Any(p => p.RefKind == RefKind.Out)))
                {
                    INamedTypeSymbol type = method.ContainingType;
                    code.Clear();

                    bool useBuffer = method.HasUseBufferAttribute();
                    string updateOverrideParameters = string.Empty;
                    List<string> parameters = new();
                    List<string> components = new();
                    HashSet<ITypeSymbol> withTypes = new(SymbolEqualityComparer.IncludeNullability);
                    HashSet<ITypeSymbol> addedTypes = new(SymbolEqualityComparer.IncludeNullability);
                    HashSet<ITypeSymbol> changedTypes = new(SymbolEqualityComparer.IncludeNullability);

                    string containerType = null;

                    if (type.IsAEntitySetSystem(out IList<ITypeSymbol> genericTypes)
                        || type.IsAEntitySortedSetSystem(out genericTypes))
                    {
                        containerType = genericTypes.Count is 1 ? "Set" : "SortedSet";
                        updateOverrideParameters = $"{GetName(genericTypes[0])} state";
                    }
                    else if (type.IsAEntityMultiMapSystem(out genericTypes))
                    {
                        containerType = "MultiMap";
                        updateOverrideParameters = $"{GetName(genericTypes[0])} state, in {GetName(genericTypes[1])} key";
                    }

                    if (containerType is null)
                    {
                        continue;
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
                        else if (containerType is "MultiMap" && SymbolEqualityComparer.Default.Equals(parameter.Type, genericTypes[1]) && parameter.RefKind != RefKind.Ref)
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

                            withTypes.Add(parameter.Type);

                            if (parameter.HasAddedAttribute())
                            {
                                addedTypes.Add(parameter.Type);
                                withTypes.Remove(parameter.Type);
                            }

                            if (parameter.HasChangedAttribute())
                            {
                                changedTypes.Add(parameter.Type);
                                withTypes.Remove(parameter.Type);
                            }

                            components.Add($"            Components<{typeName}> {name} = World.GetComponents<{typeName}>();");
                            parameters.Add($"{(parameter.RefKind == RefKind.Ref ? "ref " : string.Empty)}{name}[entity]");
                        }
                    }

                    List<INamedTypeSymbol> parentTypes = type.GetParentTypes().Skip(1).Reverse().ToList();

                    code.AppendLine("using System;")
                        .AppendLine("using System.Collections.Generic;")
                        .AppendLine("using System.Runtime.CompilerServices;")
                        .AppendLine("using DefaultEcs;")
                        .AppendLine("using DefaultEcs.Command;")
                        .AppendLine("using DefaultEcs.System;")
                        .AppendLine("using DefaultEcs.Threading;")
                        .AppendLine()
                        .Append("namespace ").AppendLine(type.GetNamespace())
                        .AppendLine("{");

                    foreach (INamedTypeSymbol parentType in parentTypes)
                    {
                        code.Append("    ").Append(parentType.DeclaredAccessibility.ToCode()).Append(" partial ").Append(parentType.TypeKind.ToCode()).Append(' ').AppendLine(parentType.GetName())
                            .AppendLine("    {");
                    }

                    code.Append("    ").Append(type.DeclaredAccessibility.ToCode()).Append(" partial class ").AppendLine(type.GetName())
                        .AppendLine("    {");

                    if (type.Constructors.All(c => c.IsImplicitlyDeclared))
                    {
                        if (useBuffer)
                        {
                            WriteConstructor(code, type, "World world", "world, CreateEntityContainer, true");
                        }
                        else if (type.IsAEntitySortedSetSystem(out IList<ITypeSymbol> _))
                        {
                            WriteConstructor(code, type, "World world", "world, CreateEntityContainer, false");
                        }
                        else
                        {
                            WriteConstructor(code, type, "World world, IParallelRunner runner, int minEntityCountByRunnerIndex", "world, CreateEntityContainer, runner, minEntityCountByRunnerIndex");
                            WriteConstructor(code, type, "World world, IParallelRunner runner", "world, CreateEntityContainer, runner, 0");
                            WriteConstructor(code, type, "World world", "world, CreateEntityContainer, null, 0");
                        }
                    }

                    WriteFactory(code, type, containerType, genericTypes.Skip(1).FirstOrDefault(), withTypes, addedTypes, changedTypes);

                    code.AppendLine("        [CompilerGenerated]")
                        .Append("        protected override void Update(").Append(updateOverrideParameters).AppendLine(", ReadOnlySpan<Entity> entities)")
                        .AppendLine("        {");

                    foreach (string component in components)
                    {
                        code.AppendLine(component);
                    }

                    if (components.Count > 0)
                    {
                        code.AppendLine();
                    }

                    code.AppendLine("            foreach (ref readonly Entity entity in entities)")
                        .AppendLine("            {")
                        .Append("                ").Append(method.Name).Append('(').Append(string.Join(", ", parameters)).AppendLine(");")
                        .AppendLine("            }")
                        .AppendLine("        }")
                        .AppendLine("    }");

                    for (int i = 0; i < parentTypes.Count; ++i)
                    {
                        code.AppendLine("    }");
                    }

                    code.AppendLine("}");

                    context.AddSource($"{type.Name}_EntitySystem{++systemCount}", code.ToString());
                }
            }
        }
    }
}
