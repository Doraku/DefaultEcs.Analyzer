using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Analyzers
{
    public class SubscribeAttributeAnalyserTest : DiagnosticVerifier
    {
        #region Tests

        [Fact]
        public void Should_not_report_DEA0001_When_ok() => VerifyCSharpDiagnostic(
@"
using DefaultEcs;

namespace DummyNamespace
{
    class DummyClass
    {
        [Subscribe]
        void DummyMethod(in bool _)
        { }
    }
}
");

        [Fact]
        public void Should_report_DEA0001_When_invalid_return_type() => VerifyCSharpDiagnostic(
@"
using DefaultEcs;

namespace DummyNamespace
{
    class DummyClass
    {
        [Subscribe]
        bool DummyMethod()
        { }
    }
}
",
            new DiagnosticResult(new(9, 14), SubscribeAttributeAnalyzer.CorrectSignatureRule, "DummyMethod"));

        [Fact]
        public void Should_report_DEA0001_When_invalid_parameters_count() => VerifyCSharpDiagnostic(
@"
using DefaultEcs;

namespace DummyNamespace
{
    class DummyClass
    {
        [Subscribe]
        void DummyMethod()
        { }
    }
}
",
            new DiagnosticResult(new(9, 14), SubscribeAttributeAnalyzer.CorrectSignatureRule, "DummyMethod"));

        [Fact]
        public void Should_report_DEA0001_When_invalid_parameter_ref_kind() => VerifyCSharpDiagnostic(
@"
using DefaultEcs;

namespace DummyNamespace
{
    class DummyClass
    {
        [Subscribe]
        void DummyMethod(bool _)
        { }
    }
}
",
            new DiagnosticResult(new(9, 14), SubscribeAttributeAnalyzer.CorrectSignatureRule, "DummyMethod"));

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new SubscribeAttributeAnalyzer();

        #endregion
    }
}
