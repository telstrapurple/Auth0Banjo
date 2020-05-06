using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services
{
    public class PipelineExecutor
    {
        public async Task ExecuteAsync(List<IProcessor<Auth0ResourceTemplate>> pipeline, IEnumerable<Auth0ResourceTemplate> templates)
        {
            foreach (var template in templates)
            {
                var result = template;
                foreach (var stage in pipeline)
                {
                    result = await stage.Process(result);
                }
            }
        }
    }
}