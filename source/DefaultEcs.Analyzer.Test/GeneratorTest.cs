using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace DefaultEcs.Analyzer.Test
{
    public class GeneratorTest
    {
        [Fact]
        public void Check()
        {
            string source = @"
using EnumValidation;
namespace Foo
{
 enum Simple
    {
        First,
        Second
    }
    enum Complex
    {
        First = 3,
        Second = 4,
        Third = 7,
        Fourth = 8,
        Fifth = 9
    }
    class C
    {
        void M(Simple simp, Complex comp)
        {
            EnumValidator.Validate(simp);
            EnumValidator.Validate(comp);
        }
    }
}";

            var (diagnostics, output) = GetGeneratedOutput(source);

            if (diagnostics.Length > 0)
            {
                Console.WriteLine("Diagnostics:");
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine("   " + diag.ToString());
                }
                Console.WriteLine();
                Console.WriteLine("Output:");
            }

            Console.WriteLine(output);
        }

        private static (ImmutableArray<Diagnostic>, string) GetGeneratedOutput(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic)
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            //ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();

            //if (diagnostics.Any())
            //{
            //    return (diagnostics, "");
            //}

            ISourceGenerator generator = new EntitySystemGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            return (diagnostics, string.Join(Environment.NewLine, outputCompilation.SyntaxTrees.Skip(1)));
        }
    }
}
