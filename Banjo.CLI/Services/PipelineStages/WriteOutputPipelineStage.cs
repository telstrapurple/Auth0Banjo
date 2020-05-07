using System.Threading.Tasks;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services.PipelineStages
{
    public class WriteOutputPipelineStage : IPipelineStage<Auth0ResourceTemplate>
    {
        private ILogger<WriteOutputPipelineStage> _logger;
        private IOptionsMonitor<Auth0ProcessArgsConfig> _args;

        public WriteOutputPipelineStage(ILogger<WriteOutputPipelineStage> logger, IOptionsMonitor<Auth0ProcessArgsConfig> args)
        {
            _logger = logger;
            _args = args;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            // _logger.LogInformation(JsonConvert.SerializeObject(t.Template, Formatting.Indented));
            return t;
        }
    }
}