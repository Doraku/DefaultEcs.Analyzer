using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DefaultEcs.Analyzer.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace DefaultEcs.Analyzer
{
    public class GeneratorTest
    {
        [Fact]
        public void Check()
        {
            const string source = @"
using System;
using System.Linq;
using DefaultEcs;
using DefaultEcs.System;
using DefaultEcs.Threading;

namespace DefaultEcs.Benchmark.DefaultEcs
{
    internal struct Position
    {
        public float X;
        public float Y;
    }

    internal struct Speed
    {
        public float X;
        public float Y;
    }

    [With(typeof(string))]
    internal sealed partial class AutoSystem<TParam> : AEntitySetSystem<float>
    {
        [ConstructorParameter]
        private readonly int _test;

        [ConstructorParameter]
        public int Gna { get; }

        [ConstructorParameter]
        public readonly int lol;

        [WorldComponent]
        private readonly int _test1;

        [WorldComponent]
        public int Gna1 { get; }

        [Update]
        private void Update(float state, ref TParam p, Dictionary<int, TParam>[] pouet, double? azd, Dictionaty<int, string> agre, in Speed speed, in Components<int> ints, [Notify] ref Position position)
        {
            position.X += speed.X * state;
            position.Y += speed.Y * state;
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
