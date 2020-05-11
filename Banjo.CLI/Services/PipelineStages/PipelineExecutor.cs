using System.Collections.Generic;
using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services.PipelineStages
{
    public class PipelineExecutor
    {
        private readonly List<IPipelineStage<Auth0ResourceTemplate>> _pipeline;

        public PipelineExecutor(List<IPipelineStage<Auth0ResourceTemplate>> pipeline)
        {
            _pipeline = pipeline;
        }

        public async Task ExecuteAsync(IEnumerable<Auth0ResourceTemplate> templates)
        {
            foreach (var template in templates)
            {
                var result = template;
                foreach (var stage in _pipeline)
                {
                    result = await stage.Process(result);
                }
            }
        }
    }
}