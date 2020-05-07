using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services.ResourceTypeProcessors;

namespace Banjo.CLI.Services.PipelineStages
{
    public class ApiExecutorPipelineStage : IPipelineStage<Auth0ResourceTemplate>
    {
        private readonly ResourceTypeProcessorFactory _processorFactory;

        public ApiExecutorPipelineStage(ResourceTypeProcessorFactory processorFactory)
        {
            _processorFactory = processorFactory;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var processor = _processorFactory.GetProcessor(t.Type);
            if (processor != null)
            {
                await processor.ProcessAsync(t);
            }

            t.ApiCallsProcessed = true;
            return t;
        }
    }
}