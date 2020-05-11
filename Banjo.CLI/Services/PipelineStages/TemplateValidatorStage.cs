using System;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services.ResourceTypeProcessors;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI.Services.PipelineStages
{
    public class TemplateValidatorStage : IPipelineStage<Auth0ResourceTemplate>
    {
        private readonly ResourceTypeProcessorFactory _processorFactory;
        private readonly ILogger<TemplateValidatorStage> _logger;

        public TemplateValidatorStage(
            ResourceTypeProcessorFactory processorFactory, 
            ILogger<TemplateValidatorStage> logger)
        {
            _processorFactory = processorFactory;
            _logger = logger;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var processor = _processorFactory.GetProcessor(t.Type);

            try
            {
                await processor.Validate(t);
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