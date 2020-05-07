using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services
{
    public class ClientsProcessor : IResourceTypeProcessor
    {
        public ResourceType[] ResourceTypes { get; } = { ResourceType.Clients };

        private readonly ILogger<ClientsProcessor> _logger;
        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, Client> _converter;
        private readonly IOptionsMonitor<Auth0ProcessArgsConfig> _args;

        public ClientsProcessor(
            ILogger<ClientsProcessor> logger,
            ManagementApiClientFactory managementApiClientFactory,
            IConverter<Auth0ResourceTemplate, Client> converter,
            IOptionsMonitor<Auth0ProcessArgsConfig> args)
        {
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
            _args = args;
            _logger = logger;
        }

        public async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var clientTemplates = _converter.Convert(template);

            // //todo support proper pagination - how to do this where every api call is different?!
            var results = await managementClient.Clients.GetAllAsync(new GetClientsRequest() { IsGlobal = false }, new PaginationInfo());

            var matchingClient = results.FirstOrDefault(x => string.Equals(x.Name, clientTemplates.Name));
            if (matchingClient == null)
            {
                //create
                _logger.LogInformation($"Creating a new client: {clientTemplates.Name}");
                var createClientRequest = Reflectorisor.CopyMembers<Client, ClientCreateRequest>(clientTemplates);

                if (_args.CurrentValue.DryRun)
                {
                    _logger.LogInformation($"DryRun flag is set. Not making the API call to Auth0 to create client {clientTemplates.Name}");
                }
                else
                {
                    // _logger.LogInformation("not doing it :-)");
                    var createResult = await managementClient.Clients.CreateAsync(createClientRequest);
                    _logger.LogInformation($"Finished creating client {createClientRequest.Name}. Created Client ID is {createResult.ClientId}");
                }
            }
            else
            {
                _logger.LogInformation($"Updating existing client: {matchingClient.ClientId} {clientTemplates.Name}");
                //update
                var updateClientRequest = Reflectorisor.CopyMembers<Client, ClientUpdateRequest>(clientTemplates);

                //fix illegal options
                if (clientTemplates.JwtConfiguration != null)
                {
                    //can't specify IsSecretEncoded (~ secret_encoded) on an update operation
                    clientTemplates.JwtConfiguration.IsSecretEncoded = null;
                }

                if (_args.CurrentValue.DryRun)
                {
                    _logger.LogInformation($"DryRun flag is set. Not making the API call to Auth0 to update client {matchingClient.ClientId} {clientTemplates.Name}");
                }
                else
                {
                    // _logger.LogInformation("not doing it :-)");
                    var updateResult = await managementClient.Clients.UpdateAsync(matchingClient.ClientId, updateClientRequest);
                    _logger.LogInformation($"Finished updating client {updateResult.Name}");
                }
            }
        }
    }
}