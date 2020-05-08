using System.Collections.Generic;
using System.Linq;
using Banjo.CLI.Services;
using Microsoft.Extensions.Configuration;

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
            var memoryProvider = _configRoot?.Providers.FirstOrDefault(x => x is ReloadingMemoryConfigurationProvider) as ReloadingMemoryConfigurationProvider;
            memoryProvider?.SetMany(new Dictionary<string, string>
            {
                { nameof(Auth0ProcessArgsConfig.DryRun), config.DryRun.ToString() },
                { nameof(Auth0ProcessArgsConfig.OutputPath), config.OutputPath },
                { nameof(Auth0ProcessArgsConfig.OverrideFilePath), config.OverrideFilePath },
                { nameof(Auth0ProcessArgsConfig.TemplateInputPath), config.TemplateInputPath },
                { nameof(Auth0ProcessArgsConfig.Verbose), config.Verbose.ToString() }
            });
        }
    }
}