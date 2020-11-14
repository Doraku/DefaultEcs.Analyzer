using DefaultEcs.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Test.Analyzers
{
    public class ComponentAttributeAnalyserTest : DiagnosticVerifier
    {
        #region Tests

        [Fact]
        public void Should_not_report_When_ok() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    [With(typeof(bool))]
    class DummyClass : AEntitySystem<float>
    {
        [WithPredicate]
        bool DummyMethod(in bool _) => true;
    }

    class DummyClass2 : AEntityBufferedSystem<float>
    {
        [WithPredicate]
        bool DummyMethod(in bool _) => true;
    }
}
");

        [Fact]
        public void Should_report_DEA0002_When_invalid_return_type() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass : AEntitySystem<float>
    {
        [WithPredicate]
        void DummyMethod()
        { }
    }
}
",
            new DiagnosticResult(new(9, 14), ComponentAttributeAnalyzer.CorrectSignatureRule, "DummyMethod"));

        [Fact]
        public void Should_report_DEA0002_When_invalid_parameters_count() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass : AEntitySystem<float>
    {
        [WithPredicate]
        bool DummyMethod() => true;
    }
}
",
            new DiagnosticResult(new(9, 14), ComponentAttributeAnalyzer.CorrectSignatureRule, "DummyMethod"));

        [Fact]
        public void Should_report_DEA0002_When_invalid_parameter_ref_kind() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass : AEntitySystem<float>
    {
        [WithPredicate]
        bool DummyMethod(bool _) => true;
    }
}
",
            new DiagnosticResult(new(9, 14), ComponentAttributeAnalyzer.CorrectSignatureRule, "DummyMethod"));

        [Fact]
        public void Should_report_DEA0003_When_invalid_base_type() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass
    {
        [WithPredicate]
        void DummyMethod()
        { }
    }
}
",
            new DiagnosticResult(new(9, 14), ComponentAttributeAnalyzer.ContainingTypeInheritEntitySystemRule, "DummyMethod"));

        [Fact]
        public void Should_report_DEA0004_When_invalid_base_type() => VerifyCSharpDiagnostic(
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    [With(typeof(bool))]
    class DummyClass
    { }
}
",
            new DiagnosticResult(new(7, 11), ComponentAttributeAnalyzer.InheritEntitySystemRule, "DummyClass", "WithAttribute"));

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ComponentAttributeAnalyzer();

        #endregion
    }
}
