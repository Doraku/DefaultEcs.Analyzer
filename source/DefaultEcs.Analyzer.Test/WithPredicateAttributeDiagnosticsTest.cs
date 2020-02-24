using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Test
{
    public class WithPredicateAttributeAnalyserTest : DiagnosticVerifier
    {
        #region Tests

        [Fact]
        public void Should_not_report_DEA0002_When_ok()
        {
            const string code =
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass
    {
        [WithPredicate]
        bool DummyMethod(in bool _) => true;
    }
}
";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void Should_report_DEA0002_When_invalid_return_type()
        {
            const string code =
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
";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = WithPredicateAttributeAnalyser.Rule.Id,
                Message = string.Format((string)WithPredicateAttributeAnalyser.Rule.MessageFormat, "DummyMethod"),
                Severity = WithPredicateAttributeAnalyser.Rule.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 14)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Fact]
        public void Should_report_DEA0002_When_invalid_parameters_count()
        {
            const string code =
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass
    {
        [WithPredicate]
        bool DummyMethod() => true;
    }
}
";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = WithPredicateAttributeAnalyser.Rule.Id,
                Message = string.Format((string)WithPredicateAttributeAnalyser.Rule.MessageFormat, "DummyMethod"),
                Severity = WithPredicateAttributeAnalyser.Rule.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 14)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Fact]
        public void Should_report_DEA0002_When_invalid_parameter_ref_kind()
        {
            const string code =
@"
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass
    {
        [WithPredicate]
        bool DummyMethod(bool _) => true;
    }
}
";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = WithPredicateAttributeAnalyser.Rule.Id,
                Message = string.Format((string)WithPredicateAttributeAnalyser.Rule.MessageFormat, "DummyMethod"),
                Severity = WithPredicateAttributeAnalyser.Rule.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 14)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new WithPredicateAttributeAnalyser();

        #endregion
    }
}
