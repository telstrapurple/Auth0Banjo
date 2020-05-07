using System;
using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services.Processors
{
    public class ApiExecutorProcessor : IProcessor<Auth0ResourceTemplate>
    {
        private readonly ResourceTypeProcessorFactory _processorFactory;

        public ApiExecutorProcessor(ResourceTypeProcessorFactory processorFactory)
        {
            _processorFactory = processorFactory;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            Console.WriteLine($"{t.Type.Name} {t.Location.FullName}");

            //skip the overrides for now, let's just get the basics working
            // - read the template AS A WHOLE PAYLOAD
            // - make the api calls
            //done

            var processor = _processorFactory.GetProcessor(t.Type);
            if (processor != null)
            {
                await processor.ProcessAsync(t);
            }

            t.ApiCallsProcessed = true;
            return t;
        }
    }
}