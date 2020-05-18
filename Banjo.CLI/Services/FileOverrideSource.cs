using System.IO;
using System.Threading.Tasks;
using AsyncLazy;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Banjo.CLI.Services
{
    public class FileOverrideSource : IOverridesSource
    {
        private readonly IReporter _reporter;
        private readonly IOptionsMonitor<Auth0ProcessArgsConfig> _config;

        public FileOverrideSource(IReporter reporter, IOptionsMonitor<Auth0ProcessArgsConfig> config)
        {
            _reporter = reporter;
            _config = config;
        }

        public async Task<Overrides> GetOverridesAsync()
        {
            var overridesFileLocation = _config.CurrentValue.OverrideFilePath;
            if (string.IsNullOrWhiteSpace(overridesFileLocation))
            {
                _reporter.Verbose("No overrides path specified (or effectively-empty path), nothing to load.");
                return new Overrides(); //empty overrides
            }

            _reporter.Verbose($"Reading overrides file from {Path.GetFullPath(overridesFileLocation)}");

            using var reader = File.OpenText(overridesFileLocation);
            using var jsonReader = new JsonTextReader(reader);
            return new JsonSerializer().Deserialize<Overrides>(jsonReader);
        }
    }
}