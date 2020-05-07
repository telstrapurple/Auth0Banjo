using System;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services.ResourceTypeProcessors;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI.Services.PipelineStages
{
    public class TemplateVerifierStage : IPipelineStage<Auth0ResourceTemplate>
    {
        private readonly ResourceTypeProcessorFactory _processorFactory;
        private readonly ILogger<TemplateVerifierStage> _logger;

        public TemplateVerifierStage(
            ResourceTypeProcessorFactory processorFactory, 
            ILogger<TemplateVerifierStage> logger)
        {
            _processorFactory = processorFactory;
            _logger = logger;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var processor = _processorFactory.GetProcessor(t.Type);

            try
            {
                processor.Verify(t);
            }
            catch (Exception e)
            {
                _logger.LogError($"Result of processing {t.Type.Name} template {t.Location.FullName} was not valid.");
                _logger.LogError(e.Message);
            }

            return t;
        }
    }
}