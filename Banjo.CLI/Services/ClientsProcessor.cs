using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Model;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI.Services
{
    public class ClientsProcessor : IResourceTypeProcessor
    {
        public ResourceType[] ResourceTypes { get; } = { ResourceType.Clients };

        private readonly ILogger<ClientsProcessor> _logger;
        private readonly ITemplateReader<Client> _clientReader;
        private readonly ManagementApiClientFactory _managementApiClientFactory;

        public ClientsProcessor(
            ILogger<ClientsProcessor> logger,
            ITemplateReader<Client> clientReader,
            ManagementApiClientFactory managementApiClientFactory
        )
        {
            _clientReader = clientReader;
            _managementApiClientFactory = managementApiClientFactory;
            _logger = logger;
        }

        public async Task ProcessAsync(TemplateMetadata template)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var clientTemplates = await _clientReader.ReadTemplateContents(template);

            //todo support proper pagination - how to do this where every api call is different?!
            var results = await managementClient.Clients.GetAllAsync(new GetClientsRequest() { IsGlobal = false }, new PaginationInfo());

            // var existingClient = results.Where(x => string.Equals(x.Name, client.Name, StringComparison.OrdinalIgnoreCase));
            foreach (var result in results)
            {
                _logger.LogInformation($"Found an existing client: {result.Name}");
            }

            if (results.Any(x => string.Equals(x.Name, clientTemplates.Name)))
            {
                _logger.LogInformation($"Found the client we're after: {clientTemplates.Name}");
            }
            else
            {
                _logger.LogInformation($"Did not find the client we're after: {clientTemplates.Name}");
            }
        }
    }
}