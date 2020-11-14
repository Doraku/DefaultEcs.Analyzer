using DefaultEcs.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace DefaultEcs.Analyzer.Test.Analyzers
{
    public class EntitySystemAnalyzerTest : DiagnosticVerifier
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
        public void Should_report_DEA0005_When_AEntitySystem()
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
            entity.Dispose();
        }
    }
}
";

            VerifyCSharpDiagnostic(
                code,
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Set"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 15, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "SetSameAs"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "NotifyChanged"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 17, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Disable"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Enable"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 19, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Disable"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 20, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Enable"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 21, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Remove"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 22, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Dispose"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 23, 13) }
                });
        }

        [Fact]
        public void Should_report_DEA0005_When_AEntitiesSystem()
        {
            const string code =
@"
using DefaultEcs;
using DefaultEcs.System;

namespace DummyNamespace
{
    class DummyClass : AEntitiesSystem<float, float>
    {
        public DummyClass(World world)
            : base(world)
        { }

        protected override void Update(float state, in float key, in Entity entity)
        {
            entity.Set(state);
            entity.SetSameAs<float>(in entity);
            entity.NotifyChanged<float>();
            entity.Disable<float>();
            entity.Enable<float>();
            entity.Disable();
            entity.Enable();
            entity.Remove<float>();
            entity.Dispose();
        }
    }
}
";

            VerifyCSharpDiagnostic(
                code,
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Set"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 15, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "SetSameAs"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "NotifyChanged"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 17, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Disable"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 18, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Enable"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 19, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Disable"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 20, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Enable"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 21, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Remove"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 22, 13) }
                },
                new DiagnosticResult
                {
                    Id = EntitySystemAnalyzer.NoEntityModificationRule.Id,
                    Message = string.Format((string)EntitySystemAnalyzer.NoEntityModificationRule.MessageFormat, "Dispose"),
                    Severity = EntitySystemAnalyzer.NoEntityModificationRule.DefaultSeverity,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 23, 13) }
                });
        }

        #endregion

        #region DiagnosticVerifier

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new EntitySystemAnalyzer();

        #endregion
    }
}
