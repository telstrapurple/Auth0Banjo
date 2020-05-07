using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace Banjo.CLI.Configuration
{
    public class ArgumentConfigurator
    {
        private readonly IConfigurationRoot _configRoot;

        public ArgumentConfigurator(IConfiguration configRoot)
        {
            _configRoot = configRoot as IConfigurationRoot;
        }

        public void AddConfiguration(Auth0ProcessArgsConfig config)
        {
            var memoryProvider = _configRoot?.Providers.FirstOrDefault(x => x.GetType() == typeof(MemoryConfigurationProvider));
            memoryProvider?.Set(nameof(Auth0ProcessArgsConfig.DryRun), config.DryRun.ToString());
            memoryProvider?.Set(nameof(Auth0ProcessArgsConfig.OutputPath), config.OutputPath);
            memoryProvider?.Set(nameof(Auth0ProcessArgsConfig.OverrideFilePath), config.OverrideFilePath);
            memoryProvider?.Set(nameof(Auth0ProcessArgsConfig.TemplateInputPath), config.TemplateInputPath);
        }
    }
}