using System;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Services
{
    public class ReplacementTokensFromEnvironmentVariablesTransformer : IOverridesTransformer
    {
        private readonly IReporter _reporter;

        public ReplacementTokensFromEnvironmentVariablesTransformer(IReporter reporter)
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

            foreach (var replacement in overrides.Replacements)
            {
                var envVar = replacement.EnvironmentVariable;
                if (string.IsNullOrEmpty(envVar))
                {
                    continue;
                }

                var value = Environment.GetEnvironmentVariable(envVar);
                if (string.IsNullOrEmpty(value))
                {
                    _reporter.Warn(
                        $"Overrides file requested environment variable {envVar} but the value was not set " +
                        "or is null or empty.");
                    _reporter.Warn("You should check the effective templates carefully for any unresolved " +
                                   "tokens.");
                    _reporter.Warn("Use argument -out|--output {output-path} to write the effective templates.");
                }

                replacement.Value = value;
            }
        }
    }
}