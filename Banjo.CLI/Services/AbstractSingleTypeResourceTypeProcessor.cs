using System;
using System.Text;
using System.Threading.Tasks;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Services.ResourceTypeProcessors;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services
{
    public abstract class AbstractSingleTypeResourceTypeProcessor<T> : IResourceTypeProcessor
    {
        public ResourceType[] ResourceTypes => new[] { Type };

        protected abstract ResourceType Type { get; }

        private readonly IOptionsMonitor<Auth0ProcessArgsConfig> _args;
        private readonly IConverter<Auth0ResourceTemplate, T> _converter;
        protected readonly IReporter Reporter;

        protected AbstractSingleTypeResourceTypeProcessor(IOptionsMonitor<Auth0ProcessArgsConfig> args, IConverter<Auth0ResourceTemplate, T> converter, IReporter reporter)
        {
            _args = args;
            _converter = converter;
            Reporter = reporter;
        }

        public virtual async Task Validate(Auth0ResourceTemplate template)
        {
            _converter.Convert(template);
        }

        public virtual async Task Preprocess(Auth0ResourceTemplate template)
        {
            //no-op
        }

        public abstract Task ProcessAsync(Auth0ResourceTemplate template);

        protected async Task Update(Func<Task<T>> updater, string id = null, string name = null)
        {
            Reporter.Output($"Updating existing {Type.Name}: {id} \"{name}\"");

            if (_args.CurrentValue.DryRun)
            {
                Reporter.Warn($"DryRun flag is set. Not making the API call to Auth0 to update {Type.Name} {id} {name}");
            }
            else
            {
                await updater.Invoke();
                Reporter.Output($"Finished updating existing {Type.Name}: {id} {name}");
            }
        }

        protected async Task Create(Func<Task<T>> creator, Func<T, string> id = null, string name = null)
        {
            Reporter.Output($"Creating a new {Type.Name}: \"{name}\"");

            if (_args.CurrentValue.DryRun)
            {
                Reporter.Warn($"DryRun flag is set. Not making the API call to Auth0 to create {Type.Name} {name}");
            }
            else
            {
                var output = await creator.Invoke();
                StringBuilder sb = new StringBuilder($"Finished creating new {Type.Name}: {name}.");
                if (id != null)
                {
                    sb.Append($" New id is {id.Invoke(output)}.");
                }

                Reporter.Output(sb.ToString());
            }
        }
    }
}