using DefaultEcs.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Test.Analyzers
{
    public class EntityCommandRecorderAttributeAnalyzerTest : DiagnosticVerifier
    {
        #region Tests

        [Fact]
        public void Should_not_report_When_ok() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.Command;
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class EntityCommandRecorderAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySystem<float>
    {
        [EntityCommandRecorderAttribute]
        private readonly EntityCommandRecorder recorder;

        [Update]
        void Update(in EntityRecord record, float state, object dummy)
        { }
    }
}
");

        [Fact]
        public void Should_report_When_do_not_inherit_entity_system() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.Command;
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class EntityCommandRecorderAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass
    {
        [EntityCommandRecorderAttribute]
        private readonly EntityCommandRecorder recorder;

        [Update]
        void Update(in EntityRecord record, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(21, 48), EntityCommandRecorderAttributeAnalyzer.InheritNonBufferedEntitySystemRule));

        [Fact]
        public void Should_report_When_do_not_have_UpdateAttribute_method() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.Command;
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class EntityCommandRecorderAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySystem<float>
    {
        [EntityCommandRecorderAttribute]
        private readonly EntityCommandRecorder recorder;

        void Update(in EntityRecord record, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(21, 48), EntityCommandRecorderAttributeAnalyzer.UpdateAttributeRule));

        [Fact]
        public void Should_report_When_do_not_have_correct_type() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.Command;
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class EntityCommandRecorderAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySystem<float>
    {
        [EntityCommandRecorderAttribute]
        private readonly int recorder;

        [Update]
        void Update(in EntityRecord record, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(21, 30), EntityCommandRecorderAttributeAnalyzer.CorrectTypeRule));

        [Fact]
        public void Should_report_When_multiple_attribute() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.Command;
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class EntityCommandRecorderAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySystem<float>
    {
        [EntityCommandRecorderAttribute]
        private readonly EntityCommandRecorder recorder;

        [EntityCommandRecorderAttribute]
        public EntityCommandRecorder Recorder { get; }

        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        void Update(in EntityRecord record, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(21, 48), EntityCommandRecorderAttributeAnalyzer.SingleEntityCommandRecorderRule),
            new DiagnosticResult(new(24, 38), EntityCommandRecorderAttributeAnalyzer.SingleEntityCommandRecorderRule));

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new EntityCommandRecorderAttributeAnalyzer();

        #endregion
    }
}
