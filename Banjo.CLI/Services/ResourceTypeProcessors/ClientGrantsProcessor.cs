using System;
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
    public class ClientGrantsProcessor : AbstractSingleTypeResourceTypeProcessor<ClientGrant>
    {
        protected override ResourceType Type { get; } = ResourceType.ClientGrants;

        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, ClientGrant> _converter;

        public ClientGrantsProcessor(
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IConverter<Auth0ResourceTemplate, ClientGrant> converter,
            IReporter reporter,
            ManagementApiClientFactory managementApiClientFactory)
            : base(args, converter, reporter)
        {
            _converter = converter;
            _managementApiClientFactory = managementApiClientFactory;
        }

        public override async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            //get the client that matches the ClientGrant.ClientId so we get Client.Id

            using var managementClient = await _managementApiClientFactory.CreateAsync();
            var templatedGrant = _converter.Convert(template);

            var allClients = await managementClient.Clients.GetAllAsync(new GetClientsRequest(), new PaginationInfo());
            var matchingClient = allClients.FirstOrDefault(x => x.Name == templatedGrant.ClientId);
            if (matchingClient == null)
            {
                throw new Auth0ResourceNotFoundException($"No {ResourceType.Clients.Name} exists with name " +
                                                         $"{templatedGrant.ClientId}. Cannot create/update " +
                                                         $"{ResourceType.ClientGrants.Name} {template.Filename}");
            }

            templatedGrant.ClientId = matchingClient.ClientId;

            var allGrants = await managementClient.ClientGrants.GetAllAsync(new GetClientGrantsRequest(), new PaginationInfo());
            var existingGrant = allGrants.FirstOrDefault(
                x => string.Equals(x.ClientId, templatedGrant.ClientId)
                     && string.Equals(x.Audience, templatedGrant.Audience));

            if (existingGrant == null)
            {
                var createRequest = Reflectorisor.CopyMembers<ClientGrant, ClientGrantCreateRequest>(templatedGrant);
                await Create(
                    async () => await managementClient.ClientGrants.CreateAsync(createRequest),
                    request => request.Id,
                    $"[{matchingClient.Name}|{createRequest.Audience}]");
            }
            else
            {
                var updateRequest = Reflectorisor.CopyMembers<ClientGrant, ClientGrantUpdateRequest>(templatedGrant);
                await Update(
                    async () => await managementClient.ClientGrants.UpdateAsync(existingGrant.Id, updateRequest),
                    existingGrant.Id,
                    $"[{matchingClient.Name}|{existingGrant.Audience}]");
            }
        }
    }
}