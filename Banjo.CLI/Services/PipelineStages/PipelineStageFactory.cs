using System;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Banjo.CLI.Services.PipelineStages
{
    public class PipelineStageFactory : IPipelineStageFactory
    {
        private readonly IServiceProvider _resolver;

        public PipelineStageFactory(IServiceProvider resolver)
        {
            _resolver = resolver;
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateTemplateReader()
        {
            return _resolver.GetRequiredService<TemplateReaderPipelineStage>();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateOverridesProcessor()
        {
            return _resolver.GetRequiredService<ApplyOverridesPipelineStage>();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateOutputProcessor()
        {
            return _resolver.GetRequiredService<WriteOutputPipelineStage>();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateApiExecutor()
        {
            return _resolver.GetRequiredService<ApiExecutorPipelineStage>();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateVerifier()
        {
            return _resolver.GetRequiredService<TemplateValidatorStage>();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateUnresolvedTokenVerifier()
        {
            return _resolver.GetRequiredService<UnresolvedTokenVerifierStage>();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateTokenReplacementStage()
        {
            return _resolver.GetRequiredService<StringReplacementStage>();
        }

        public IPipelineStage<Auth0ResourceTemplate> CreateResourcePreprocessingStage()
        {
            return _resolver.GetRequiredService<ResourcePreProcessorStage>();
        }
    }
}