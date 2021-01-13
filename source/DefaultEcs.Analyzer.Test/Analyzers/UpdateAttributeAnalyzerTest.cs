using DefaultEcs.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Test.Analyzers
{
    public class UpdateAttributeAnalyzerTest : DiagnosticVerifier
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

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class AddedAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class ChangedAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        void Update(in Entity entity, float state, [Added] string c1, [Changed] double c2)
        { }
    }
}
");

        [Fact]
        public void Should_report_When_not_partial() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(13, 11), UpdateAttributeAnalyzer.PartialTypeRule));

        [Fact]
        public void Should_report_When_all_containing_type_not_partial() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    class DummyParent
    {
    partial class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }
    }
    }
}
",
            new DiagnosticResult(new(13, 11), UpdateAttributeAnalyzer.PartialTypeRule));

        [Fact]
        public void Should_report_When_update_override() => VerifyCSharpDiagnostic(
@"
using DefaultEcs;
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        protected override void Update(float state, in Entity entity)
        { }

        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(24, 14), UpdateAttributeAnalyzer.NoUpdateOverrideRule));

        [Fact]
        public void Should_report_When_multiple_update() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }

        [Update]
        void Update(in Entity entity)
        { }
    }
}
",
            new DiagnosticResult(new(20, 14), UpdateAttributeAnalyzer.SingleUpdateAttributeRule),
            new DiagnosticResult(new(24, 14), UpdateAttributeAnalyzer.SingleUpdateAttributeRule));

        [Fact]
        public void Should_report_When_not_entity_system() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass
    {
        [Update]
        void Update(in Entity entity, float state, object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(16, 14), UpdateAttributeAnalyzer.InheritEntitySystemRule));

        [Fact]
        public void Should_report_When_not_void() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        int Update(in Entity entity, float state, object dummy) => 42;
    }
}
",
            new DiagnosticResult(new(20, 13), UpdateAttributeAnalyzer.VoidReturnRule));

        [Fact]
        public void Should_report_When_out_parameter() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        void Update(in Entity entity, float state, out object dummy)
        { }
    }
}
",
            new DiagnosticResult(new(20, 14), UpdateAttributeAnalyzer.NoOutParameterRule));

        [Fact]
        public void Should_report_When_generic() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        [Update]
        void Update<T>(in Entity entity, float state)
        { }
    }
}
",
            new DiagnosticResult(new(20, 14), UpdateAttributeAnalyzer.NoGenericRule));

        [Fact]
        public void Should_report_When_no_Update_attribute() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class AddedAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class ChangedAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        void Update(in Entity entity, float state, [Added] string c1, [Changed] double c2)
        { }
    }
}
",
            new DiagnosticResult(new(23, 67), UpdateAttributeAnalyzer.UpdateAttributeRule),
            new DiagnosticResult(new(23, 88), UpdateAttributeAnalyzer.UpdateAttributeRule));

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new UpdateAttributeAnalyzer();

        #endregion
    }
}
