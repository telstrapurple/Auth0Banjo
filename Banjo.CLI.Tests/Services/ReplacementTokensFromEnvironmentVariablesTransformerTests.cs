using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services;
using DeepEqual.Syntax;
using Shouldly;
using Xunit;

namespace Banjo.CLI.Tests.Services
{
    public class ReplacementTokensFromEnvironmentVariablesTransformerTests
    {
        [Fact]
        public async Task TestEmptyOverridesShouldRemainEmpty()
        {
            var reporter = new TestReporter();
            var emptyOverrides = new Overrides { Replacements = new List<ReplacementDefinition>() };

            var sut = new ReplacementTokensFromEnvironmentVariablesTransformer(reporter);
            await sut.TransformAsync(emptyOverrides);

            emptyOverrides.Replacements.ShouldBeEmpty();
        }

        [Fact]
        public async Task TestOverridesWithOnlyVerbatimReplacementsShouldRemainUnchanged()
        {
            var reporter = new TestReporter();
            var tokenValuePairs = new Dictionary<string, string> { { "herpa", "derpa" }, { "thomas", "fromtidmouth" } };
            var overrides = CreateFrom(tokenValuePairs);
            var overridesClone = CreateFrom(tokenValuePairs);

            var sut = new ReplacementTokensFromEnvironmentVariablesTransformer(reporter);
            await sut.TransformAsync(overrides);

            overrides.ShouldDeepEqual(overridesClone);
        }

        [Fact]
        public async Task TestEnvironmentVariablesAreRead()
        {
            var reporter = new TestReporter();
            var overrides = new Overrides
            {
                Replacements = new List<ReplacementDefinition>
                {
                    new ReplacementDefinition { Token = "herpa", EnvironmentVariable = "HERPA" },
                    new ReplacementDefinition { Token = "thomas", EnvironmentVariable = "THOMAS" }
                }
            };
            Environment.SetEnvironmentVariable("HERPA", "derpa");
            Environment.SetEnvironmentVariable("THOMAS", "fromtidmouth");

            var sut = new ReplacementTokensFromEnvironmentVariablesTransformer(reporter);
            await sut.TransformAsync(overrides);

            var expectedReplacements = new List<ReplacementDefinition>
            {
                new ReplacementDefinition { Token = "herpa", EnvironmentVariable = "HERPA", Value = "derpa" },
                new ReplacementDefinition { Token = "thomas", EnvironmentVariable = "THOMAS", Value = "fromtidmouth" }
            };

            overrides.Replacements.ShouldDeepEqual(expectedReplacements);
        }

        [Fact]
        public async Task TestValuesShouldBeOverriddenByEnvVars()
        {
            var reporter = new TestReporter();
            var overrides = new Overrides
            {
                Replacements = new List<ReplacementDefinition>
                {
                    new ReplacementDefinition { Token = "herpa", EnvironmentVariable = "HERPA", Value = "IShouldBeReplaced" },
                    new ReplacementDefinition { Token = "thomas", EnvironmentVariable = "THOMAS", Value = "AndMeToo" }
                }
            };
            try
            {
                Environment.SetEnvironmentVariable("HERPA", "derpa");
                Environment.SetEnvironmentVariable("THOMAS", "fromtidmouth");

                var sut = new ReplacementTokensFromEnvironmentVariablesTransformer(reporter);
                await sut.TransformAsync(overrides);

                var expectedReplacements = new List<ReplacementDefinition>
                {
                    new ReplacementDefinition { Token = "herpa", EnvironmentVariable = "HERPA", Value = "derpa" },
                    new ReplacementDefinition { Token = "thomas", EnvironmentVariable = "THOMAS", Value = "fromtidmouth" }
                };

                overrides.Replacements.ShouldDeepEqual(expectedReplacements);
            }
            finally
            {
                Environment.SetEnvironmentVariable("HERPA", null);
                Environment.SetEnvironmentVariable("THOMAS", null);
            }
        }

        [Fact]
        public async Task TestUnsetEnvVarEmitsWarningMessage()
        {
            var reporter = new TestReporter();
            var overrides = new Overrides
            {
                Replacements = new List<ReplacementDefinition>
                {
                    new ReplacementDefinition { Token = "herpa", EnvironmentVariable = "HERPA" },
                }
            };

            var sut = new ReplacementTokensFromEnvironmentVariablesTransformer(reporter);
            await sut.TransformAsync(overrides);

            reporter.WarnMessages.ShouldNotBeEmpty();
        }

        private static Overrides CreateFrom(Dictionary<string, string> tokenValuePairs)
        {
            return new Overrides
            {
                Replacements = tokenValuePairs.Select(kv => new ReplacementDefinition() { Token = kv.Key, Value = kv.Value }).ToList()
            };
        }
    }
}