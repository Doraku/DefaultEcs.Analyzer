using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Analyzers
{
    public class WorldComponentAttributeAnalyzerTest : DiagnosticVerifier
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
    internal sealed class WorldComponentAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        [WorldComponent]
        private readonly int field;

        [WorldComponent]
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
    internal sealed class WorldComponentAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass
    {
        [WorldComponent]
        private readonly int field;

        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(20, 30), WorldComponentAttributeAnalyzer.InheritEntitySystemRule));

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
    internal sealed class WorldComponentAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        [WorldComponent]
        private readonly int field;

        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(20, 30), WorldComponentAttributeAnalyzer.UpdateAttributeRule));

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
    internal sealed class WorldComponentAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        [WorldComponent]
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
            new DiagnosticResult(new(20, 30), WorldComponentAttributeAnalyzer.NoConstructorRule));

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new WorldComponentAttributeAnalyzer();

        #endregion
    }
}
