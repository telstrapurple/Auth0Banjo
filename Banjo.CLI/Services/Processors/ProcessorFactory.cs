using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services.Processors
{
    public class ProcessorFactory
    {
        private readonly IOptionsMonitor<Auth0ProcessArgsConfig> _argOptions;
        private readonly IOverridesSource _overridesSource;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ResourceTypeProcessorFactory _resourceTypeProcessorFactory;

        public ProcessorFactory(
            IOptionsMonitor<Auth0ProcessArgsConfig> argOptions,
            IOverridesSource overridesSource,
            ILoggerFactory loggerFactory,
            ResourceTypeProcessorFactory resourceTypeProcessorFactory)
        {
            _argOptions = argOptions;
            _overridesSource = overridesSource;
            _loggerFactory = loggerFactory;
            _resourceTypeProcessorFactory = resourceTypeProcessorFactory;
        }

        public IProcessor<Auth0ResourceTemplate> CreateTemplateReader()
        {
            return new TemplateReaderProcessor();
        }

        public IProcessor<Auth0ResourceTemplate> CreateOverridesProcessor()
        {
            var overrides = _overridesSource.GetOverrides(_argOptions.CurrentValue.OverrideFilePath);
            return new ApplyOverridesProcessor(overrides);
        }

        public IProcessor<Auth0ResourceTemplate> CreateOutputProcessor()
        {
            return new WriteOutputProcessor(_loggerFactory.CreateLogger<WriteOutputProcessor>());
        }

        public IProcessor<Auth0ResourceTemplate> CreateApiExecutor()
        {
            return new ApiExecutorProcessor(_resourceTypeProcessorFactory);
        }
    }
}