using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Test
{
    public class AEntitySystemDiagnosticsTest : DiagnosticVerifier
    {
        #region Tests

        [Fact]
        public void Should_not_report_When_ok()
        {
            const string code =
@"
using DefaultEcs;
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass : AEntitySystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        protected override void Update(float state, in Entity entity)
        {
            entity.Get<float>();
        }
    }
}
";

            VerifyCSharpDiagnostic(code);
        }

        [Fact]
        public void Should_report_DEA0005_When_invalid_parameter_ref_kind()
        {
            const string code =
@"
using DefaultEcs;
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass : AEntitySystem<float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        protected override void Update(float state, in Entity entity)
        {
            entity.Set(state);
            entity.SetSameAs<float>(in entity);
            entity.NotifyChanged<float>();
            entity.Disable<float>();
            entity.Enable<float>();
            entity.Disable();
            entity.Enable();
            entity.Remove<float>();
            entity.SetAsChildOf(entity);
            entity.RemoveFromChildrenOf(entity);
            entity.SetAsParentOf(entity);
            entity.RemoveFromParentsOf(entity);
            entity.Dispose();
        }
    }
}
";

            VerifyCSharpDiagnostic(
                code,
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "Set"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 15, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "SetSameAs"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "NotifyChanged"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 17, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "Disable"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "Enable"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 19, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "Disable"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 20, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "Enable"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 21, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "Remove"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 22, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "SetAsChildOf"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 23, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "RemoveFromChildrenOf"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 24, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "SetAsParentOf"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 25, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "RemoveFromParentsOf"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 26, 13) }
                },
                new DiagnosticResult
                {
                    Id = AEntitySystemAnalyser.Rule.Id,
                    Message = string.Format((string)AEntitySystemAnalyser.Rule.MessageFormat, "Dispose"),
                    Severity = AEntitySystemAnalyser.Rule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 27, 13) }
                });
        }

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new AEntitySystemAnalyser();

        #endregion
    }
}
