using System;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services.ResourceTypeProcessors;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Services.PipelineStages
{
    public class ResourcePreProcessorStage : IPipelineStage<Auth0ResourceTemplate>
    {
        public string Name { get; } = "Preprocess Template";

        private readonly IResourceTypeProcessorFactory _processorFactory;
        private readonly IReporter _reporter;

        public ResourcePreProcessorStage(
            IResourceTypeProcessorFactory processorFactory, 
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
                await processor.Preprocess(t);
                t.Preprocessed = true;
            }
            catch (Exception e)
            {
                _reporter.Error($"Result of preprocessing {t.Type.Name} template {t.Location.FullName} failed with an error.");
                _reporter.Error(e.Message);
            }

            return t;
        }
    }
}