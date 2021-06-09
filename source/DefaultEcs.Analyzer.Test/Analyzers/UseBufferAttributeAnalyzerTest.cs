using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Analyzers
{
    public class UseBufferAttributeAnalyzerTest : DiagnosticVerifier
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

    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UseBufferAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        [Update, UseBuffer]
        void Update(in Entity entity)
        { }
    }
}
");

        [Fact]
        public void Should_report_When_has_constructor() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DefaultEcs.System
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class UpdateAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class UseBufferAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        [Update, UseBuffer]
        void Update(in Entity entity)
        { }
    }
}
",
            new DiagnosticResult(new(24, 14), UseBufferAttributeAnalyzer.NoConstructorRule));

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
    internal sealed class UseBufferAttribute : Attribute
    { }
}

namespace DummyNamespace
{
    partial class DummyClass : AEntitySetSystem<float>
    {
        [UseBuffer]
        void Update(in Entity entity)
        { }
    }
}
",
            new DiagnosticResult(new(20, 14), UseBufferAttributeAnalyzer.UpdateAttributeRule));

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new UseBufferAttributeAnalyzer();

        #endregion
    }
}
