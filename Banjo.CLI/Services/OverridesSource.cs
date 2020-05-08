using System.Collections.Generic;
using System.IO;
using System.Linq;
using Banjo.CLI.Model;
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
        private readonly ILogger<OverridesSource> _logger;

        public OverridesSource(ILogger<OverridesSource> logger)
        {
            _logger = logger;
        }

        public Overrides GetOverrides(string overridesFileLocation)
        {
            if (string.IsNullOrWhiteSpace(overridesFileLocation))
            {
                return new Overrides(); //empty overrides
            }

            using var reader = File.OpenText(overridesFileLocation);
            using var jsonReader = new JsonTextReader(reader);
            var overrides = new JsonSerializer().Deserialize<Overrides>(jsonReader);
            
            if ((overrides?.Replacements?.Count ?? 0) > 0)
            {
                Dictionary<string, string> newReplacements = new Dictionary<string, string>();
                
                foreach (var (key, value) in overrides.Replacements)
                {
                    newReplacements.TryAdd($"%%{key}%%", value);
                    newReplacements.TryAdd($"%%{key.ToUpperInvariant()}%%", value);
                    newReplacements.TryAdd($"%%{key.ToLowerInvariant()}%%", value);
                }
                
                overrides.Replacements = newReplacements;
            }
            
            return overrides;
        }
    }
}