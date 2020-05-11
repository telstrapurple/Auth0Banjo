using System;
using System.Threading.Tasks;
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
            return new ApiExecutorPipelineStage(_resourceTypeProcessorFactory, _argOptions, _reporter);
        }
        
        public IPipelineStage<Auth0ResourceTemplate> CreateVerifier()
        {
            return new TemplateValidatorStage(_resourceTypeProcessorFactory, _loggerFactory.CreateLogger<TemplateValidatorStage>());
        }
        
        public IPipelineStage<Auth0ResourceTemplate> CreateTokenReplacementStage()
        {
            return new StringReplacementStage();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateResourcePreprocessingStage()
        {
            return new ResourcePreProcessorStage(_resourceTypeProcessorFactory, _reporter);
        }
    }

    public class ResourcePreProcessorStage : IPipelineStage<Auth0ResourceTemplate>
    {
        private readonly ResourceTypeProcessorFactory _processorFactory;
        private readonly IReporter _reporter;

        public ResourcePreProcessorStage(
            ResourceTypeProcessorFactory processorFactory, 
            IReporter reporter)
        {
            _processorFactory = processorFactory;
            _reporter = reporter;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var processor = _processorFactory.GetProcessor(t.Type);

            try
            {
                await processor.Preprocess(t);
                t.Preprocessed = true;
            }
            catch (Exception e)
            {
                _reporter.Error($"Result of preprocessing {t.Type.Name} template {t.Location.FullName} failed with an error.");
                _reporter.Error(e.Message);
            }

            return t;
        }
    }
}