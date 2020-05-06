using System.Threading.Tasks;
using Banjo.CLI.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Banjo.CLI.Services.Enrichers
{
    public class WriteOutputProcessor : IProcessor<Auth0ResourceTemplate>
    {
        private ILogger<WriteOutputProcessor> _logger;

        public WriteOutputProcessor(ILogger<WriteOutputProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(t.Template, Formatting.Indented));
            return t;
        }
    }
}