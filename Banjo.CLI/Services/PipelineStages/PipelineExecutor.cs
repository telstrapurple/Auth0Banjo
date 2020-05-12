using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Services.PipelineStages
{
    public class PipelineExecutor
    {
        private readonly List<IPipelineStage<Auth0ResourceTemplate>> _pipeline;
        private IReporter _reporter;

        public PipelineExecutor(
            List<IPipelineStage<Auth0ResourceTemplate>> pipeline,
            IReporter reporter)
        {
            _pipeline = pipeline;
            _reporter = reporter;
        }

        public async Task ExecuteAsync(IEnumerable<Auth0ResourceTemplate> templates)
        {
            foreach (var template in templates)
            {
                var result = template;
                foreach (var stage in _pipeline)
                {
                    _reporter.Verbose($"Processing {template.Type.Name} template {template.Filename} through stage {stage.Name}");
                    try
                    {
                        result = await stage.Process(result);
                    }
                    catch (Exception e)
                    {
                        _reporter.Error($"An error occurred processing {template.Type.Name} template {template.Filename} in stage \'{stage.Name}\'");
                        _reporter.Error(e.Message);
                        _reporter.Error("Template cannot be processed further.");
                        break;
                    }
                }
            }
        }
    }
}