using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services.PipelineStages
{
    public interface IPipelineStageFactory
    {
        IPipelineStage<Auth0ResourceTemplate> CreateTemplateReader();
        IPipelineStage<Auth0ResourceTemplate> CreateOverridesProcessor();
        IPipelineStage<Auth0ResourceTemplate> CreateOutputProcessor();
        IPipelineStage<Auth0ResourceTemplate> CreateApiExecutor();
        IPipelineStage<Auth0ResourceTemplate> CreateVerifier();
        IPipelineStage<Auth0ResourceTemplate> CreateUnresolvedTokenVerifier();
        IPipelineStage<Auth0ResourceTemplate> CreateTokenReplacementStage();
        IPipelineStage<Auth0ResourceTemplate> CreateResourcePreprocessingStage();
    }
}