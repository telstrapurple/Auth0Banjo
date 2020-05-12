using System.Collections.Generic;
using System.IO;
using System.Linq;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Banjo.CLI.Services
{
    public interface IOverridesSource
    {
        Overrides GetOverrides(string overridesFileLocation);
    }

    public class OverridesSource : IOverridesSource
    {
        private readonly IReporter _reporter;

        public OverridesSource(IReporter reporter)
        {
            _reporter = reporter;
        }

        public Overrides GetOverrides(string overridesFileLocation)
        {
            if (string.IsNullOrWhiteSpace(overridesFileLocation))
            {
                _reporter.Verbose("No overrides path specified (or effectively-empty path), nothing to load.");
                return new Overrides(); //empty overrides
            }
            
            _reporter.Verbose($"Reading overrides file from {Path.GetFullPath(overridesFileLocation)}");

            using var reader = File.OpenText(overridesFileLocation);
            using var jsonReader = new JsonTextReader(reader);
            var overrides = new JsonSerializer().Deserialize<Overrides>(jsonReader);

            if ((overrides?.Replacements?.Count ?? 0) == 0) return overrides;
            
            var newReplacements = new Dictionary<string, string>();
                
            foreach (var (key, value) in overrides.Replacements)
            {
                newReplacements.TryAdd($"%%{key}%%", value);
                newReplacements.TryAdd($"%%{key.ToUpperInvariant()}%%", value);
                newReplacements.TryAdd($"%%{key.ToLowerInvariant()}%%", value);
            }
                
            overrides.Replacements = newReplacements;

            return overrides;
        }
    }
}