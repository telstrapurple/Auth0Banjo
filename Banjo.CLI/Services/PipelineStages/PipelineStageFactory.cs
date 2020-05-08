using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Services.ResourceTypeProcessors;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services.PipelineStages
{
    public class PipelineStageFactory
    {
        private readonly IOptionsMonitor<Auth0ProcessArgsConfig> _argOptions;
        private readonly IOverridesSource _overridesSource;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ResourceTypeProcessorFactory _resourceTypeProcessorFactory;
        private readonly IReporter _reporter;

        public PipelineStageFactory(
            IOptionsMonitor<Auth0ProcessArgsConfig> argOptions,
            IOverridesSource overridesSource,
            ILoggerFactory loggerFactory,
            ResourceTypeProcessorFactory resourceTypeProcessorFactory, 
            IReporter reporter)
        {
            _argOptions = argOptions;
            _overridesSource = overridesSource;
            _loggerFactory = loggerFactory;
            _resourceTypeProcessorFactory = resourceTypeProcessorFactory;
            _reporter = reporter;
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateTemplateReader()
        {
            return new TemplateReaderPipelineStage();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateOverridesProcessor()
        {
            var overrides = _overridesSource.GetOverrides(_argOptions.CurrentValue.OverrideFilePath);
            return new ApplyOverridesPipelineStage(overrides);
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateOutputProcessor()
        {
            return new WriteOutputPipelineStage(_reporter, _argOptions);
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateApiExecutor()
        {
            return new ApiExecutorPipelineStage(_resourceTypeProcessorFactory);
        }
        
        public IPipelineStage<Auth0ResourceTemplate> CreateVerifier()
        {
            return new TemplateVerifierStage(_resourceTypeProcessorFactory, _loggerFactory.CreateLogger<TemplateVerifierStage>());
        }
    }
}