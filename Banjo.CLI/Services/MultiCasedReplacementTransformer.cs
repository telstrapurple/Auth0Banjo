using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Services
{
    public class MultiCasedReplacementTransformer : IOverridesTransformer
    {
        private readonly IReporter _reporter;

        public MultiCasedReplacementTransformer(IReporter reporter)
        {
            _reporter = reporter;
        }

        public async Task TransformAsync(Overrides overrides)
        {
            if (!(overrides?.Replacements?.Any() ?? false))
            {
                _reporter.Verbose("Overrides source does not contain any replacements, no replacement tokens to generate casing variations for.");
                return; //no replacements, nothing to do.
            }

            var newReplacements = overrides.Replacements
                .Where(x => x.Value != null) //we want to allow empty values to be replaced, just not null values.
                .SelectMany(x =>
            {
                var _ = new List<ReplacementDefinition>
                {
                    new ReplacementDefinition() { Token = $"%%{x.Token}%%", Value = x.Value, EnvironmentVariable = x.EnvironmentVariable },
                    new ReplacementDefinition() { Token = $"%%{x.Token.ToUpperInvariant()}%%", Value = x.Value, EnvironmentVariable = x.EnvironmentVariable },
                    new ReplacementDefinition() { Token = $"%%{x.Token.ToLowerInvariant()}%%", Value = x.Value, EnvironmentVariable = x.EnvironmentVariable }
                };
                return _;
            }).Distinct(ReplacementDefinition.TokenComparer);

            overrides.Replacements = newReplacements;

            foreach (var r in overrides.Replacements)
            {
                string rValue;

                if (string.IsNullOrEmpty(r.EnvironmentVariable)) rValue = r.Value;
                else rValue = string.IsNullOrEmpty(r.Value) ? "" : "****";

                _reporter.Verbose($"Generated replacement [token = {r.Token}, value = {rValue}, env-var = {r.EnvironmentVariable}]");
            }
        }
    }
}