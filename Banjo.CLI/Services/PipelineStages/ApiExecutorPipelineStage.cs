using System.Threading.Tasks;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Services.ResourceTypeProcessors;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services.PipelineStages
{
    public class ApiExecutorPipelineStage : IPipelineStage<Auth0ResourceTemplate>
    {
        public string Name { get; } = "Execute Auth0 API";

        private readonly ResourceTypeProcessorFactory _processorFactory;
        private readonly IOptionsMonitor<Auth0ProcessArgsConfig> _args;
        private readonly IReporter _reporter;

        public ApiExecutorPipelineStage(
            ResourceTypeProcessorFactory processorFactory,
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IReporter reporter)
        {
            _processorFactory = processorFactory;
            _args = args;
            _reporter = reporter;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var processor = _processorFactory.GetProcessor(t.Type);
            if (processor != null)
            {
                try
                {
                    _reporter.Output($"Executing API calls to provision {t.Type.Name} for template {t.Filename}");

                    await processor.ProcessAsync(t);
                }
                catch (Auth0ResourceNotFoundException e)
                {
                    if (_args.CurrentValue.DryRun)
                    {
                        _reporter.Warn("A required Auth0 resource was not found in order to plan the Auth0 API calls.");
                        _reporter.Warn("Rerunning this command without the -d | --dry-run flag may cause the required resource(s) to be provisioned, allowing this resource to succeed.");
                        _reporter.Warn($"Message: {e.Message}");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            t.ApiCallsProcessed = true;
            return t;
        }
    }
}