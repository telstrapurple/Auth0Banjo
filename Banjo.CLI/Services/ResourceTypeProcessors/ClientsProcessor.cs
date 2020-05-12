using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public class ClientsProcessor : AbstractSingleTypeResourceTypeProcessor<Client>
    {
        protected override ResourceType Type { get; } = ResourceType.Clients;

        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, Client> _converter;

        public ClientsProcessor(
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IConverter<Auth0ResourceTemplate, Client> converter,
            IReporter reporter, ManagementApiClientFactory managementApiClientFactory)
            : base(args, converter, reporter)
        {
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
        }

        public override async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var client = _converter.Convert(template);

            //todo support proper pagination - how to do this where every api call is different?!
            var getClientsRequest = new GetClientsRequest
            {
                IsGlobal = false, IncludeFields = true, Fields = "name,client_id" 
                
            };
            var results = await managementClient.Clients.GetAllAsync(getClientsRequest, new PaginationInfo());

            FixIllegalOptions(client);

            var matchingClient = results.FirstOrDefault(x => string.Equals(x.Name, client.Name));
            if (matchingClient == null)
            {
                var createRequest = Reflectorisor.CopyMembers<Client, ClientCreateRequest>(client);
                await Create(
                    async () => await managementClient.Clients.CreateAsync(createRequest),
                    request => request.ClientId,
                    client.Name);
            }
            else
            {
                var updateRequest = Reflectorisor.CopyMembers<Client, ClientUpdateRequest>(client);
                await Update(
                    async () => await managementClient.Clients.UpdateAsync(matchingClient.ClientId, updateRequest),
                    matchingClient.ClientId,
                    client.Name
                );
            }
        }

        private static void FixIllegalOptions(Client client)
        {
            if (client.JwtConfiguration != null)
            {
                //can't specify IsSecretEncoded (~ secret_encoded) on an update operation
                client.JwtConfiguration.IsSecretEncoded = null;
            }
        }
    }
}