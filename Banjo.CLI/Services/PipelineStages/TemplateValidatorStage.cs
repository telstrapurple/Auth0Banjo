using System;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services.ResourceTypeProcessors;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI.Services.PipelineStages
{
    public class TemplateValidatorStage : IPipelineStage<Auth0ResourceTemplate>
    {
        public string Name { get; } = "Validate Template";

        private readonly ResourceTypeProcessorFactory _processorFactory;
        private readonly IReporter _reporter;

        public TemplateValidatorStage(
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
                await processor.Validate(t);
            }
            catch (Exception e)
            {
                _reporter.Error($"Result of processing {t.Type.Name} template {t.Location.FullName} was not valid.");
                _reporter.Error(e.Message);
            }

            return t;
        }
    }
}