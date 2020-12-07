using DefaultEcs.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Test.Analyzers
{
    public class ConstructorParameterAttributeAnalyzerTest : DiagnosticVerifier
    {
        #region Tests

        [Fact]
        public void Should_not_report_When_ok() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class ConstructorParameterAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySystem<float>
    {
        [ConstructorParameter]
        private readonly int field;

        [ConstructorParameter]
        private readonly int Property { get; }

        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
");

        [Fact]
        public void Should_report_When_do_not_inherit_entity_system() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class ConstructorParameterAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass
    {
        [ConstructorParameter]
        private readonly int field;

        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(20, 30), ConstructorParameterAttributeAnalyzer.InheritEntitySystemRule));

        [Fact]
        public void Should_report_When_do_not_have_UpdateAttribute_method() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class ConstructorParameterAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySystem<float>
    {
        [ConstructorParameter]
        private readonly int field;

        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(20, 30), ConstructorParameterAttributeAnalyzer.HasUpdateAttributeSystemRule));

        [Fact]
        public void Should_report_When_has_constructor() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class ConstructorParameterAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySystem<float>
    {
        [ConstructorParameter]
        private readonly int field;

        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(20, 30), ConstructorParameterAttributeAnalyzer.HasNoConstructorRule));

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ConstructorParameterAttributeAnalyzer();

        #endregion
    }
}
