using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Diagnostics.Test
{
    public class ComponentAttributeAnalyserTest : DiagnosticVerifier
    {
        #region Tests

        [Fact]
        public void Should_not_report_When_ok()
        {
            const string code =
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    [With(typeof(bool))]
    class DummyClass : AEntitySystem<float>
    { }
}
";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void Should_report_DEA0004_When_invalid_base_type()
        {
            const string code =
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    [With(typeof(bool))]
    class DummyClass
    { }
}
";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = ComponentAttributeAnalyzer.InvalidBaseTypeRule.Id,
                Message = string.Format((string)ComponentAttributeAnalyzer.InvalidBaseTypeRule.MessageFormat, "DummyClass", "WithAttribute"),
                Severity = ComponentAttributeAnalyzer.InvalidBaseTypeRule.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 11)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ComponentAttributeAnalyzer();

        #endregion
    }
}
