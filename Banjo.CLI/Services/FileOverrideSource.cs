using System.IO;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Banjo.CLI.Services
{
    public class FileOverrideSource : IOverridesSource
    {
        private readonly IReporter _reporter;

        public FileOverrideSource(IReporter reporter)
        {
            _reporter = reporter;
        }

        public async Task<Overrides> GetOverridesAsync(string overridesFileLocation)
        {
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