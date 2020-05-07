using System.IO;
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
            return new JsonSerializer().Deserialize<Overrides>(jsonReader);
        }
    }
}