using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Test
{
    public class SubscribeAttributeAnalyserTest : DiagnosticVerifier
    {
        #region Tests

        [Fact]
        public void Should_not_report_DEA0001_When_ok()
        {
            const string code =
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
";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void Should_report_DEA0001_When_invalid_return_type()
        {
            const string code =
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
";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = SubscribeAttributeAnalyser.Rule.Id,
                Message = string.Format((string)SubscribeAttributeAnalyser.Rule.MessageFormat, "DummyMethod"),
                Severity = SubscribeAttributeAnalyser.Rule.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 14)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Fact]
        public void Should_report_DEA0001_When_invalid_parameters_count()
        {
            const string code =
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
";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = SubscribeAttributeAnalyser.Rule.Id,
                Message = string.Format((string)SubscribeAttributeAnalyser.Rule.MessageFormat, "DummyMethod"),
                Severity = SubscribeAttributeAnalyser.Rule.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 14)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Fact]
        public void Should_report_DEA0001_When_invalid_parameter_ref_kind()
        {
            const string code =
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
";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = SubscribeAttributeAnalyser.Rule.Id,
                Message = string.Format((string)SubscribeAttributeAnalyser.Rule.MessageFormat, "DummyMethod"),
                Severity = SubscribeAttributeAnalyser.Rule.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 14)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new SubscribeAttributeAnalyser();

        #endregion
    }
}
